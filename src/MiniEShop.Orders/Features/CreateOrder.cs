namespace MiniEShop.Orders.Features;

using System.Net.Http.Json;
using MediatR;
using MiniEShop.Orders.Domain.Entities;
using MiniEShop.Orders.Domain.Repositories;
using Microsoft.Extensions.Logging;

public static class CreateOrder
{
    public record OrderItemRequest(string ProductId, int Quantity);
    
    public record Command(
        string UserId,
        IEnumerable<OrderItemRequest> Items) : IRequest<Response>;

    public record Response(
        bool Success,
        string? OrderId = null,
        string? ErrorMessage = null);

    public class Handler : IRequestHandler<Command, Response>
    {
        private readonly IOrderRepository _repository;
        private readonly HttpClient _httpClient;
        private readonly ILogger<Handler> _logger;

        public Handler(
            IOrderRepository repository,
            IHttpClientFactory httpClientFactory,
            ILogger<Handler> logger)
        {
            _repository = repository;
            _httpClient = httpClientFactory.CreateClient("InventoryService");
            _logger = logger;
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                // Get product details and prices
                var orderItems = new List<OrderItem>();
                foreach (var item in request.Items)
                {
                    var productResponse = await _httpClient.GetFromJsonAsync<ProductDto>(
                        $"api/products/{item.ProductId}", cancellationToken);

                    if (productResponse == null)
                    {
                        return new Response(false, ErrorMessage: $"Product not found: {item.ProductId}");
                    }

                    orderItems.Add(new OrderItem(item.ProductId, item.Quantity, productResponse.Price));
                }

                // Try to reserve inventory
                var reservationRequest = new
                {
                    Items = request.Items.Select(i => new { i.ProductId, i.Quantity })
                };

                var reservationResponse = await _httpClient.PostAsJsonAsync(
                    "api/inventory/reserve", reservationRequest, cancellationToken);

                if (!reservationResponse.IsSuccessStatusCode)
                {
                    var error = await reservationResponse.Content.ReadFromJsonAsync<ReservationError>(cancellationToken);
                    return new Response(false, ErrorMessage: error?.ErrorMessage ?? "Failed to reserve inventory");
                }

                // Create order
                var order = new Order(request.UserId, orderItems);
                await _repository.CreateAsync(order);

                // Confirm order
                order.MarkAsConfirmed();
                await _repository.UpdateAsync(order);

                _logger.LogInformation("Created order {OrderId} for user {UserId}", order.Id, request.UserId);
                return new Response(true, order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for user {UserId}", request.UserId);
                return new Response(false, ErrorMessage: "An error occurred while processing your order");
            }
        }

        private record ProductDto(string Id, string Name, decimal Price);
        private record ReservationError(string ErrorMessage);
    }
}

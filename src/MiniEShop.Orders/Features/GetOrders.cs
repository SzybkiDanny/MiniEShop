namespace MiniEShop.Orders.Features;

using MediatR;
using MiniEShop.Orders.Domain.Entities;
using MiniEShop.Orders.Domain.Repositories;
using Microsoft.Extensions.Logging;

public static class GetOrders
{
    public record Query(string? UserId = null) : IRequest<Response>;

    public record Response(IEnumerable<OrderDto> Orders);

    public record OrderItemDto(
        string ProductId,
        int Quantity,
        decimal UnitPrice,
        decimal TotalPrice);

    public record OrderDto(
        string Id,
        string UserId,
        string Status,
        decimal TotalAmount,
        DateTime CreatedAt,
        IEnumerable<OrderItemDto> Items);

    public class Handler : IRequestHandler<Query, Response>
    {
        private readonly IOrderRepository _repository;
        private readonly ILogger<Handler> _logger;

        public Handler(IOrderRepository repository, ILogger<Handler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                var orders = request.UserId != null
                    ? await _repository.GetByUserIdAsync(request.UserId)
                    : await _repository.GetAllAsync();

                var orderDtos = orders.Select(MapOrderToDto);
                return new Response(orderDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders for user {UserId}", request.UserId ?? "all");
                throw;
            }
        }

        private static OrderDto MapOrderToDto(Order order)
        {
            var itemDtos = order.Items.Select(item => new OrderItemDto(
                item.ProductId,
                item.Quantity,
                item.UnitPrice,
                item.Quantity * item.UnitPrice));

            return new OrderDto(
                order.Id,
                order.UserId,
                order.Status.ToString(),
                order.TotalAmount,
                order.CreatedAt,
                itemDtos);
        }
    }
}

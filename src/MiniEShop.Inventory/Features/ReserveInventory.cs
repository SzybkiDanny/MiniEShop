namespace MiniEShop.Inventory.Features;

using MediatR;
using MiniEShop.Inventory.Domain.Repositories;
using Microsoft.Extensions.Logging;

public static class ReserveInventory
{
    public record ReservationItem(string ProductId, int Quantity);

    public record Command(IEnumerable<ReservationItem> Items) : IRequest<Response>;

    public record Response(bool Success, string? ErrorMessage = null);

    public class Handler : IRequestHandler<Command, Response>
    {
        private readonly IInventoryRepository _repository;
        private readonly ILogger<Handler> _logger;

        public Handler(IInventoryRepository repository, ILogger<Handler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Attempting to reserve inventory for {Count} products", request.Items.Count());

                foreach (var item in request.Items)
                {
                    var success = await _repository.TryReserveAsync(item.ProductId, item.Quantity);
                    if (!success)
                    {
                        _logger.LogWarning("Failed to reserve {Quantity} items for product {ProductId}", 
                            item.Quantity, item.ProductId);

                        // Cancel all previous reservations
                        foreach (var rollbackItem in request.Items.TakeWhile(x => x != item))
                        {
                            await _repository.CancelReservationAsync(rollbackItem.ProductId, rollbackItem.Quantity);
                        }

                        return new Response(false, $"Insufficient inventory for product {item.ProductId}");
                    }
                }

                _logger.LogInformation("Successfully reserved inventory for all products");
                return new Response(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reserving inventory");
                return new Response(false, "An error occurred while reserving inventory");
            }
        }
    }
}

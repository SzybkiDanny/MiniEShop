namespace MiniEShop.Inventory.Features;

using MediatR;
using MiniEShop.Inventory.Domain.Repositories;
using Microsoft.Extensions.Logging;

public static class GetInventoryByProductId
{
    public record Query(string ProductId) : IRequest<Response>;

    public record Response(InventoryDto? Inventory);

    public record InventoryDto(
        string ProductId,
        int Quantity,
        int Reserved,
        int Available);

    public class Handler : IRequestHandler<Query, Response>
    {
        private readonly IInventoryRepository _repository;
        private readonly ILogger<Handler> _logger;

        public Handler(IInventoryRepository repository, ILogger<Handler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting inventory for product: {ProductId}", request.ProductId);

            var item = await _repository.GetByProductIdAsync(request.ProductId);
            if (item == null)
            {
                _logger.LogWarning("Inventory not found for product: {ProductId}", request.ProductId);
                return new Response(null);
            }

            var dto = new InventoryDto(
                item.ProductId,
                item.Quantity,
                item.Reserved,
                item.Quantity - item.Reserved);

            return new Response(dto);
        }
    }
}

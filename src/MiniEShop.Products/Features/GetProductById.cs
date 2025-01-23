namespace MiniEShop.Products.Features;

using MediatR;
using MiniEShop.Products.Domain.Repositories;
using Microsoft.Extensions.Logging;

public static class GetProductById
{
    public record Query(string Id) : IRequest<Response>;

    public record Response(ProductDto? Product);

    public record ProductDto(
        string Id,
        string Name,
        string Description,
        decimal Price,
        string Category,
        string ImageUrl);

    public class Handler : IRequestHandler<Query, Response>
    {
        private readonly IProductRepository _repository;
        private readonly ILogger<Handler> _logger;

        public Handler(IProductRepository repository, ILogger<Handler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting product with ID: {ProductId}", request.Id);

            var product = await _repository.GetByIdAsync(request.Id);
            if (product == null)
            {
                _logger.LogWarning("Product not found with ID: {ProductId}", request.Id);
                return new Response(null);
            }

            var productDto = new ProductDto(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.Category,
                product.ImageUrl);

            return new Response(productDto);
        }
    }
}

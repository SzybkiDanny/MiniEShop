namespace MiniEShop.Products.Features;

using MediatR;
using MiniEShop.Products.Domain.Repositories;
using Microsoft.Extensions.Logging;

public static class GetProducts
{
    public record Query(string? NameLike = null) : IRequest<Response>;

    public record Response(IEnumerable<ProductDto> Products);

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
            _logger.LogInformation("Getting products with name filter: {NameFilter}", request.NameLike);

            var products = string.IsNullOrWhiteSpace(request.NameLike)
                ? await _repository.GetAllAsync()
                : await _repository.GetByNameAsync(request.NameLike);

            var productDtos = products.Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Category,
                p.ImageUrl));

            return new Response(productDtos);
        }
    }
}

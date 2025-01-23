namespace MiniEShop.Products.Infrastructure.Repositories;

using Azure;
using Azure.Data.Tables;
using MiniEShop.Products.Domain.Entities;
using MiniEShop.Products.Domain.Repositories;
using MiniEShop.Products.Infrastructure.TableEntities;
using Microsoft.Extensions.Logging;

public class ProductRepository : IProductRepository
{
    private readonly TableClient _tableClient;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(string connectionString, ILogger<ProductRepository> logger)
    {
        _tableClient = new TableClient(connectionString, "Products");
        _logger = logger;
        _tableClient.CreateIfNotExists();
    }

    public async Task<Product?> GetByIdAsync(string id)
    {
        try
        {
            var results = _tableClient.QueryAsync<ProductTableEntity>(x => x.RowKey == id);
            await foreach (var entity in results)
            {
                return entity.ToProduct();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product with ID {ProductId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        try
        {
            var products = new List<Product>();
            var results = _tableClient.QueryAsync<ProductTableEntity>();
            await foreach (var entity in results)
            {
                products.Add(entity.ToProduct());
            }
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products");
            throw;
        }
    }

    public async Task<IEnumerable<Product>> GetByNameAsync(string nameLike)
    {
        try
        {
            var products = new List<Product>();
            var results = _tableClient.QueryAsync<ProductTableEntity>(
                entity => entity.Name.Contains(nameLike, StringComparison.OrdinalIgnoreCase));
            
            await foreach (var entity in results)
            {
                products.Add(entity.ToProduct());
            }
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products with name like {NameLike}", nameLike);
            throw;
        }
    }

    public async Task CreateAsync(Product product)
    {
        try
        {
            var entity = ProductTableEntity.FromProduct(product);
            await _tableClient.AddEntityAsync(entity);
            _logger.LogInformation("Created product with ID {ProductId}", product.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product with ID {ProductId}", product.Id);
            throw;
        }
    }

    public async Task UpdateAsync(Product product)
    {
        try
        {
            var entity = ProductTableEntity.FromProduct(product);
            await _tableClient.UpdateEntityAsync(entity, ETag.All);
            _logger.LogInformation("Updated product with ID {ProductId}", product.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID {ProductId}", product.Id);
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            var product = await GetByIdAsync(id);
            if (product != null)
            {
                await _tableClient.DeleteEntityAsync(product.Category, id);
                _logger.LogInformation("Deleted product with ID {ProductId}", id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with ID {ProductId}", id);
            throw;
        }
    }
}

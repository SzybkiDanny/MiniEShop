namespace MiniEShop.Products.Infrastructure.TableEntities;

using Azure;
using Azure.Data.Tables;
using MiniEShop.Products.Domain.Entities;

public class ProductTableEntity : ITableEntity
{
    public string PartitionKey { get; set; } = default!;
    public string RowKey { get; set; } = default!;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public string Category { get; set; } = default!;
    public string ImageUrl { get; set; } = default!;

    public static ProductTableEntity FromProduct(Product product)
    {
        return new ProductTableEntity
        {
            PartitionKey = product.Category,
            RowKey = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Category = product.Category,
            ImageUrl = product.ImageUrl
        };
    }

    public Product ToProduct()
    {
        return new Product(
            Name,
            Description,
            Price,
            Category,
            ImageUrl)
        {
            Id = RowKey
        };
    }
}

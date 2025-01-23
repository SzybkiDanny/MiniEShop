namespace MiniEShop.Products.Domain.Repositories;

using MiniEShop.Products.Domain.Entities;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(string id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<IEnumerable<Product>> GetByNameAsync(string nameLike);
    Task CreateAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(string id);
}

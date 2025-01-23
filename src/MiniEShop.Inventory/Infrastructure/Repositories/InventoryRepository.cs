namespace MiniEShop.Inventory.Infrastructure.Repositories;

using Azure;
using Azure.Data.Tables;
using MiniEShop.Inventory.Domain.Entities;
using MiniEShop.Inventory.Domain.Repositories;
using MiniEShop.Inventory.Infrastructure.TableEntities;
using Microsoft.Extensions.Logging;

public class InventoryRepository : IInventoryRepository
{
    private readonly TableClient _tableClient;
    private readonly ILogger<InventoryRepository> _logger;

    public InventoryRepository(string connectionString, ILogger<InventoryRepository> logger)
    {
        _tableClient = new TableClient(connectionString, "Inventory");
        _logger = logger;
        _tableClient.CreateIfNotExists();
    }

    public async Task<InventoryItem?> GetByProductIdAsync(string productId)
    {
        try
        {
            var results = _tableClient.QueryAsync<InventoryTableEntity>(x => x.RowKey == productId);
            await foreach (var entity in results)
            {
                return entity.ToInventoryItem();
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory for product {ProductId}", productId);
            throw;
        }
    }

    public async Task<IEnumerable<InventoryItem>> GetAllAsync()
    {
        try
        {
            var items = new List<InventoryItem>();
            var results = _tableClient.QueryAsync<InventoryTableEntity>();
            await foreach (var entity in results)
            {
                items.Add(entity.ToInventoryItem());
            }
            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all inventory items");
            throw;
        }
    }

    public async Task CreateAsync(InventoryItem item)
    {
        try
        {
            var entity = InventoryTableEntity.FromInventoryItem(item);
            await _tableClient.AddEntityAsync(entity);
            _logger.LogInformation("Created inventory item for product {ProductId}", item.ProductId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating inventory item for product {ProductId}", item.ProductId);
            throw;
        }
    }

    public async Task UpdateAsync(InventoryItem item)
    {
        try
        {
            var entity = InventoryTableEntity.FromInventoryItem(item);
            await _tableClient.UpdateEntityAsync(entity, ETag.All);
            _logger.LogInformation("Updated inventory item for product {ProductId}", item.ProductId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating inventory item for product {ProductId}", item.ProductId);
            throw;
        }
    }

    public async Task DeleteAsync(string productId)
    {
        try
        {
            await _tableClient.DeleteEntityAsync("STOCK", productId);
            _logger.LogInformation("Deleted inventory item for product {ProductId}", productId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting inventory item for product {ProductId}", productId);
            throw;
        }
    }

    public async Task<bool> TryReserveAsync(string productId, int amount)
    {
        try
        {
            var item = await GetByProductIdAsync(productId);
            if (item == null || !item.CanReserve(amount))
            {
                return false;
            }

            item.Reserve(amount);
            await UpdateAsync(item);
            _logger.LogInformation("Reserved {Amount} items for product {ProductId}", amount, productId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving {Amount} items for product {ProductId}", amount, productId);
            throw;
        }
    }

    public async Task ConfirmReservationAsync(string productId, int amount)
    {
        try
        {
            var item = await GetByProductIdAsync(productId);
            if (item == null)
            {
                throw new InvalidOperationException($"Inventory item not found for product {productId}");
            }

            item.ConfirmReservation(amount);
            await UpdateAsync(item);
            _logger.LogInformation("Confirmed reservation of {Amount} items for product {ProductId}", amount, productId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming reservation of {Amount} items for product {ProductId}", amount, productId);
            throw;
        }
    }

    public async Task CancelReservationAsync(string productId, int amount)
    {
        try
        {
            var item = await GetByProductIdAsync(productId);
            if (item == null)
            {
                throw new InvalidOperationException($"Inventory item not found for product {productId}");
            }

            item.CancelReservation(amount);
            await UpdateAsync(item);
            _logger.LogInformation("Cancelled reservation of {Amount} items for product {ProductId}", amount, productId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling reservation of {Amount} items for product {ProductId}", amount, productId);
            throw;
        }
    }
}

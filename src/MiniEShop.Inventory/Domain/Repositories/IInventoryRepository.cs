namespace MiniEShop.Inventory.Domain.Repositories;

using MiniEShop.Inventory.Domain.Entities;

public interface IInventoryRepository
{
    Task<InventoryItem?> GetByProductIdAsync(string productId);
    Task<IEnumerable<InventoryItem>> GetAllAsync();
    Task CreateAsync(InventoryItem item);
    Task UpdateAsync(InventoryItem item);
    Task DeleteAsync(string productId);
    Task<bool> TryReserveAsync(string productId, int amount);
    Task ConfirmReservationAsync(string productId, int amount);
    Task CancelReservationAsync(string productId, int amount);
}

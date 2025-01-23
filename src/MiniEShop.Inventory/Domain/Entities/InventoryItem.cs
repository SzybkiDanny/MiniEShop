namespace MiniEShop.Inventory.Domain.Entities;

public class InventoryItem
{
    public string ProductId { get; internal set; }
    public int Quantity { get; private set; }
    public int Reserved { get; private set; }

    public InventoryItem(string productId, int quantity)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));
        
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(quantity));

        ProductId = productId;
        Quantity = quantity;
        Reserved = 0;
    }

    public bool CanReserve(int amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        return (Quantity - Reserved) >= amount;
    }

    public void Reserve(int amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        if (!CanReserve(amount))
            throw new InvalidOperationException($"Cannot reserve {amount} items. Only {Quantity - Reserved} available.");

        Reserved += amount;
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity < Reserved)
            throw new InvalidOperationException($"Cannot set quantity to {newQuantity} as {Reserved} items are reserved.");

        Quantity = newQuantity;
    }

    public void CancelReservation(int amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        if (amount > Reserved)
            throw new InvalidOperationException($"Cannot cancel reservation of {amount} items as only {Reserved} are reserved.");

        Reserved -= amount;
    }

    public void ConfirmReservation(int amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        if (amount > Reserved)
            throw new InvalidOperationException($"Cannot confirm reservation of {amount} items as only {Reserved} are reserved.");

        Reserved -= amount;
        Quantity -= amount;
    }
}

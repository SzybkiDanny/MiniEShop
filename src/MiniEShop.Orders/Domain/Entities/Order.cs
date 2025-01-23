namespace MiniEShop.Orders.Domain.Entities;

public class Order
{
    public string Id { get; internal set; }
    public string UserId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    private readonly List<OrderItem> _items;
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Order(string userId, IEnumerable<OrderItem> items)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (!items.Any())
            throw new ArgumentException("Order must contain at least one item", nameof(items));

        Id = Guid.NewGuid().ToString();
        UserId = userId;
        Status = OrderStatus.Created;
        CreatedAt = DateTime.UtcNow;
        _items = items.ToList();
        TotalAmount = _items.Sum(i => i.Quantity * i.UnitPrice);
    }

    public void MarkAsConfirmed()
    {
        if (Status != OrderStatus.Created)
            throw new InvalidOperationException($"Cannot confirm order in {Status} status");

        Status = OrderStatus.Confirmed;
    }

    public void MarkAsFailed()
    {
        if (Status != OrderStatus.Created)
            throw new InvalidOperationException($"Cannot mark order as failed in {Status} status");

        Status = OrderStatus.Failed;
    }
}

public class OrderItem
{
    public string ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    public OrderItem(string productId, int quantity, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));
        
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));
        
        if (unitPrice <= 0)
            throw new ArgumentException("Unit price must be positive", nameof(unitPrice));

        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}

public enum OrderStatus
{
    Created,
    Confirmed,
    Failed
}

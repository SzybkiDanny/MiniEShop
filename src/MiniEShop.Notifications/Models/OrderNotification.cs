namespace MiniEShop.Notifications.Models;

public class OrderNotification
{
    public string OrderId { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<OrderItemNotification> Items { get; set; } = default!;
}

public class OrderItemNotification
{
    public string ProductId { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

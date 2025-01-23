namespace MiniEShop.Orders.Infrastructure.TableEntities;

using Azure;
using Azure.Data.Tables;
using MiniEShop.Orders.Domain.Entities;

public class OrderTableEntity : ITableEntity
{
    public string PartitionKey { get; set; } = default!;  // UserId
    public string RowKey { get; set; } = default!;        // OrderId
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    
    public string Status { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }

    public static OrderTableEntity FromOrder(Order order)
    {
        return new OrderTableEntity
        {
            PartitionKey = order.UserId,
            RowKey = order.Id,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt
        };
    }

    public Order ToOrder(IEnumerable<OrderItem> items)
    {
        var order = new Order(PartitionKey, items)
        {
            Id = RowKey
        };

        // Set the correct status
        if (Enum.TryParse<OrderStatus>(Status, out var status))
        {
            switch (status)
            {
                case OrderStatus.Confirmed:
                    order.MarkAsConfirmed();
                    break;
                case OrderStatus.Failed:
                    order.MarkAsFailed();
                    break;
            }
        }

        return order;
    }
}

public class OrderItemTableEntity : ITableEntity
{
    public string PartitionKey { get; set; } = default!;  // OrderId
    public string RowKey { get; set; } = default!;        // ProductId
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public static OrderItemTableEntity FromOrderItem(string orderId, OrderItem item)
    {
        return new OrderItemTableEntity
        {
            PartitionKey = orderId,
            RowKey = item.ProductId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice
        };
    }

    public OrderItem ToOrderItem()
    {
        return new OrderItem(RowKey, Quantity, UnitPrice);
    }
}

namespace MiniEShop.Inventory.Infrastructure.TableEntities;

using Azure;
using Azure.Data.Tables;
using MiniEShop.Inventory.Domain.Entities;

public class InventoryTableEntity : ITableEntity
{
    public string PartitionKey { get; set; } = default!;
    public string RowKey { get; set; } = default!;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    
    public int Quantity { get; set; }
    public int Reserved { get; set; }

    public static InventoryTableEntity FromInventoryItem(InventoryItem item)
    {
        return new InventoryTableEntity
        {
            PartitionKey = "STOCK",
            RowKey = item.ProductId,
            Quantity = item.Quantity,
            Reserved = item.Reserved
        };
    }

    public InventoryItem ToInventoryItem()
    {
        var item = new InventoryItem(RowKey, Quantity);
        
        // If there are reserved items, we need to simulate the reservations
        if (Reserved > 0)
        {
            item.Reserve(Reserved);
        }

        return item;
    }
}

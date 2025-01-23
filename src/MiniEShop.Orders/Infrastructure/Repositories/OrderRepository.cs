namespace MiniEShop.Orders.Infrastructure.Repositories;

using Azure;
using Azure.Data.Tables;
using MiniEShop.Orders.Domain.Entities;
using MiniEShop.Orders.Domain.Repositories;
using MiniEShop.Orders.Infrastructure.TableEntities;
using Microsoft.Extensions.Logging;

public class OrderRepository : IOrderRepository
{
    private readonly TableClient _orderTableClient;
    private readonly TableClient _orderItemTableClient;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(string connectionString, ILogger<OrderRepository> logger)
    {
        _orderTableClient = new TableClient(connectionString, "Orders");
        _orderItemTableClient = new TableClient(connectionString, "OrderItems");
        _logger = logger;

        _orderTableClient.CreateIfNotExists();
        _orderItemTableClient.CreateIfNotExists();
    }

    public async Task<Order?> GetByIdAsync(string id)
    {
        try
        {
            var orderResults = _orderTableClient.QueryAsync<OrderTableEntity>(x => x.RowKey == id);
            OrderTableEntity? orderEntity = null;
            await foreach (var entity in orderResults)
            {
                orderEntity = entity;
                break;
            }

            if (orderEntity == null)
            {
                return null;
            }

            var itemResults = _orderItemTableClient.QueryAsync<OrderItemTableEntity>(x => x.PartitionKey == id);
            var items = new List<OrderItem>();
            await foreach (var entity in itemResults)
            {
                items.Add(entity.ToOrderItem());
            }

            return orderEntity.ToOrder(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order with ID {OrderId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(string userId)
    {
        try
        {
            var orders = new List<Order>();
            var orderResults = _orderTableClient.QueryAsync<OrderTableEntity>(x => x.PartitionKey == userId);
            
            await foreach (var orderEntity in orderResults)
            {
                var itemResults = _orderItemTableClient.QueryAsync<OrderItemTableEntity>(x => x.PartitionKey == orderEntity.RowKey);
                var items = new List<OrderItem>();
                await foreach (var itemEntity in itemResults)
                {
                    items.Add(itemEntity.ToOrderItem());
                }

                orders.Add(orderEntity.ToOrder(items));
            }

            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        try
        {
            var orders = new List<Order>();
            var orderResults = _orderTableClient.QueryAsync<OrderTableEntity>();
            
            await foreach (var orderEntity in orderResults)
            {
                var itemResults = _orderItemTableClient.QueryAsync<OrderItemTableEntity>(x => x.PartitionKey == orderEntity.RowKey);
                var items = new List<OrderItem>();
                await foreach (var itemEntity in itemResults)
                {
                    items.Add(itemEntity.ToOrderItem());
                }

                orders.Add(orderEntity.ToOrder(items));
            }

            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all orders");
            throw;
        }
    }

    public async Task CreateAsync(Order order)
    {
        try
        {
            var orderEntity = OrderTableEntity.FromOrder(order);
            await _orderTableClient.AddEntityAsync(orderEntity);

            foreach (var item in order.Items)
            {
                var itemEntity = OrderItemTableEntity.FromOrderItem(order.Id, item);
                await _orderItemTableClient.AddEntityAsync(itemEntity);
            }

            _logger.LogInformation("Created order with ID {OrderId}", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order with ID {OrderId}", order.Id);
            throw;
        }
    }

    public async Task UpdateAsync(Order order)
    {
        try
        {
            var orderEntity = OrderTableEntity.FromOrder(order);
            await _orderTableClient.UpdateEntityAsync(orderEntity, ETag.All);

            _logger.LogInformation("Updated order with ID {OrderId}", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order with ID {OrderId}", order.Id);
            throw;
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            var order = await GetByIdAsync(id);
            if (order != null)
            {
                // Delete order items first
                var itemResults = _orderItemTableClient.QueryAsync<OrderItemTableEntity>(x => x.PartitionKey == id);
                await foreach (var item in itemResults)
                {
                    await _orderItemTableClient.DeleteEntityAsync(item.PartitionKey, item.RowKey);
                }

                // Then delete the order
                await _orderTableClient.DeleteEntityAsync(order.UserId, id);
                _logger.LogInformation("Deleted order with ID {OrderId}", id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order with ID {OrderId}", id);
            throw;
        }
    }
}

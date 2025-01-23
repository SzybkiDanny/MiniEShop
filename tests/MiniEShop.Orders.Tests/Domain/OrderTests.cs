namespace MiniEShop.Orders.Tests.Domain;

using Xunit;
using MiniEShop.Orders.Domain.Entities;

public class OrderTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateOrder()
    {
        // Arrange
        var userId = "test-user";
        var items = new[]
        {
            new OrderItem("product-1", 2, 10.0m),
            new OrderItem("product-2", 1, 20.0m)
        };

        // Act
        var order = new Order(userId, items);

        // Assert
        Assert.Equal(userId, order.UserId);
        Assert.Equal(OrderStatus.Created, order.Status);
        Assert.Equal(40.0m, order.TotalAmount); // (2 * 10) + (1 * 20)
        Assert.NotEmpty(order.Id);
        Assert.Equal(2, order.Items.Count);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidUserId_ShouldThrowException(string invalidUserId)
    {
        // Arrange
        var items = new[] { new OrderItem("product-1", 1, 10.0m) };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Order(invalidUserId, items));
        
        Assert.Equal("User ID cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyItems_ShouldThrowException()
    {
        // Arrange
        var items = Array.Empty<OrderItem>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Order("user-1", items));
        
        Assert.Equal("Order must contain at least one item", exception.Message);
    }

    [Fact]
    public void MarkAsConfirmed_WhenCreated_ShouldConfirmOrder()
    {
        // Arrange
        var order = new Order("user-1", new[] { new OrderItem("product-1", 1, 10.0m) });

        // Act
        order.MarkAsConfirmed();

        // Assert
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void MarkAsConfirmed_WhenAlreadyConfirmed_ShouldThrowException()
    {
        // Arrange
        var order = new Order("user-1", new[] { new OrderItem("product-1", 1, 10.0m) });
        order.MarkAsConfirmed();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            order.MarkAsConfirmed());
        
        Assert.Contains("Cannot confirm order in Confirmed status", exception.Message);
    }

    [Fact]
    public void MarkAsFailed_WhenCreated_ShouldFailOrder()
    {
        // Arrange
        var order = new Order("user-1", new[] { new OrderItem("product-1", 1, 10.0m) });

        // Act
        order.MarkAsFailed();

        // Assert
        Assert.Equal(OrderStatus.Failed, order.Status);
    }

    [Fact]
    public void MarkAsFailed_WhenAlreadyFailed_ShouldThrowException()
    {
        // Arrange
        var order = new Order("user-1", new[] { new OrderItem("product-1", 1, 10.0m) });
        order.MarkAsFailed();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            order.MarkAsFailed());
        
        Assert.Contains("Cannot mark order as failed in Failed status", exception.Message);
    }
}

public class OrderItemTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateOrderItem()
    {
        // Arrange
        var productId = "test-product";
        var quantity = 2;
        var unitPrice = 10.0m;

        // Act
        var item = new OrderItem(productId, quantity, unitPrice);

        // Assert
        Assert.Equal(productId, item.ProductId);
        Assert.Equal(quantity, item.Quantity);
        Assert.Equal(unitPrice, item.UnitPrice);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidProductId_ShouldThrowException(string invalidProductId)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new OrderItem(invalidProductId, 1, 10.0m));
        
        Assert.Equal("Product ID cannot be empty", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidQuantity_ShouldThrowException(int invalidQuantity)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new OrderItem("product-1", invalidQuantity, 10.0m));
        
        Assert.Equal("Quantity must be positive", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidUnitPrice_ShouldThrowException(decimal invalidUnitPrice)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new OrderItem("product-1", 1, invalidUnitPrice));
        
        Assert.Equal("Unit price must be positive", exception.Message);
    }
}

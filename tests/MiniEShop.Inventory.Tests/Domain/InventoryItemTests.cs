namespace MiniEShop.Inventory.Tests.Domain;

using Xunit;
using MiniEShop.Inventory.Domain.Entities;

public class InventoryItemTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateInventoryItem()
    {
        // Arrange
        var productId = "test-product";
        var quantity = 10;

        // Act
        var item = new InventoryItem(productId, quantity);

        // Assert
        Assert.Equal(productId, item.ProductId);
        Assert.Equal(quantity, item.Quantity);
        Assert.Equal(0, item.Reserved);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidProductId_ShouldThrowException(string invalidProductId)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new InventoryItem(invalidProductId, 10));
        
        Assert.Equal("Product ID cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_WithNegativeQuantity_ShouldThrowException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new InventoryItem("test-product", -1));
        
        Assert.Equal("Quantity cannot be negative", exception.Message);
    }

    [Fact]
    public void CanReserve_WithAvailableQuantity_ShouldReturnTrue()
    {
        // Arrange
        var item = new InventoryItem("test-product", 10);

        // Act
        var canReserve = item.CanReserve(5);

        // Assert
        Assert.True(canReserve);
    }

    [Fact]
    public void CanReserve_WithInsufficientQuantity_ShouldReturnFalse()
    {
        // Arrange
        var item = new InventoryItem("test-product", 10);
        item.Reserve(8);

        // Act
        var canReserve = item.CanReserve(5);

        // Assert
        Assert.False(canReserve);
    }

    [Fact]
    public void Reserve_WithAvailableQuantity_ShouldReserveItems()
    {
        // Arrange
        var item = new InventoryItem("test-product", 10);

        // Act
        item.Reserve(5);

        // Assert
        Assert.Equal(5, item.Reserved);
        Assert.Equal(10, item.Quantity);
    }

    [Fact]
    public void Reserve_WithInsufficientQuantity_ShouldThrowException()
    {
        // Arrange
        var item = new InventoryItem("test-product", 10);
        item.Reserve(8);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            item.Reserve(5));
        
        Assert.Contains("Cannot reserve 5 items", exception.Message);
    }

    [Fact]
    public void UpdateQuantity_WithValidQuantity_ShouldUpdateQuantity()
    {
        // Arrange
        var item = new InventoryItem("test-product", 10);
        item.Reserve(5);

        // Act
        item.UpdateQuantity(15);

        // Assert
        Assert.Equal(15, item.Quantity);
        Assert.Equal(5, item.Reserved);
    }

    [Fact]
    public void UpdateQuantity_WithQuantityLessThanReserved_ShouldThrowException()
    {
        // Arrange
        var item = new InventoryItem("test-product", 10);
        item.Reserve(5);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            item.UpdateQuantity(3));
        
        Assert.Contains("Cannot set quantity to 3", exception.Message);
    }

    [Fact]
    public void CancelReservation_WithValidAmount_ShouldCancelReservation()
    {
        // Arrange
        var item = new InventoryItem("test-product", 10);
        item.Reserve(5);

        // Act
        item.CancelReservation(3);

        // Assert
        Assert.Equal(2, item.Reserved);
        Assert.Equal(10, item.Quantity);
    }

    [Fact]
    public void CancelReservation_WithAmountGreaterThanReserved_ShouldThrowException()
    {
        // Arrange
        var item = new InventoryItem("test-product", 10);
        item.Reserve(5);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            item.CancelReservation(8));
        
        Assert.Contains("Cannot cancel reservation of 8 items", exception.Message);
    }

    [Fact]
    public void ConfirmReservation_WithValidAmount_ShouldConfirmReservation()
    {
        // Arrange
        var item = new InventoryItem("test-product", 10);
        item.Reserve(5);

        // Act
        item.ConfirmReservation(3);

        // Assert
        Assert.Equal(2, item.Reserved);
        Assert.Equal(7, item.Quantity);
    }

    [Fact]
    public void ConfirmReservation_WithAmountGreaterThanReserved_ShouldThrowException()
    {
        // Arrange
        var item = new InventoryItem("test-product", 10);
        item.Reserve(5);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            item.ConfirmReservation(8));
        
        Assert.Contains("Cannot confirm reservation of 8 items", exception.Message);
    }
}

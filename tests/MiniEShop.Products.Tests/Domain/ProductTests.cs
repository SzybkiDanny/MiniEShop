namespace MiniEShop.Products.Tests.Domain;

using Xunit;
using MiniEShop.Products.Domain.Entities;

public class ProductTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateProduct()
    {
        // Arrange
        var name = "Test Product";
        var description = "Test Description";
        var price = 10.0m;
        var category = "Test Category";
        var imageUrl = "http://test.com/image.jpg";

        // Act
        var product = new Product(name, description, price, category, imageUrl);

        // Assert
        Assert.Equal(name, product.Name);
        Assert.Equal(description, product.Description);
        Assert.Equal(price, product.Price);
        Assert.Equal(category, product.Category);
        Assert.Equal(imageUrl, product.ImageUrl);
        Assert.NotEmpty(product.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidName_ShouldThrowException(string invalidName)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Product(invalidName, "description", 10.0m, "category", "imageUrl"));
        
        Assert.Equal("Product name cannot be empty", exception.Message);
    }

    [Fact]
    public void Constructor_WithNegativePrice_ShouldThrowException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            new Product("name", "description", -10.0m, "category", "imageUrl"));
        
        Assert.Equal("Price cannot be negative", exception.Message);
    }

    [Fact]
    public void UpdatePrice_WithValidPrice_ShouldUpdatePrice()
    {
        // Arrange
        var product = new Product("name", "description", 10.0m, "category", "imageUrl");
        var newPrice = 15.0m;

        // Act
        product.UpdatePrice(newPrice);

        // Assert
        Assert.Equal(newPrice, product.Price);
    }

    [Fact]
    public void UpdatePrice_WithNegativePrice_ShouldThrowException()
    {
        // Arrange
        var product = new Product("name", "description", 10.0m, "category", "imageUrl");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => product.UpdatePrice(-15.0m));
        Assert.Equal("Price cannot be negative", exception.Message);
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdateDetails()
    {
        // Arrange
        var product = new Product("name", "description", 10.0m, "category", "imageUrl");
        var newName = "New Name";
        var newDescription = "New Description";
        var newCategory = "New Category";

        // Act
        product.UpdateDetails(newName, newDescription, newCategory);

        // Assert
        Assert.Equal(newName, product.Name);
        Assert.Equal(newDescription, product.Description);
        Assert.Equal(newCategory, product.Category);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UpdateDetails_WithInvalidName_ShouldThrowException(string invalidName)
    {
        // Arrange
        var product = new Product("name", "description", 10.0m, "category", "imageUrl");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            product.UpdateDetails(invalidName, "new description", "new category"));
        
        Assert.Equal("Product name cannot be empty", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UpdateImage_WithInvalidImageUrl_ShouldThrowException(string invalidImageUrl)
    {
        // Arrange
        var product = new Product("name", "description", 10.0m, "category", "imageUrl");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            product.UpdateImage(invalidImageUrl));
        
        Assert.Equal("Image URL cannot be empty", exception.Message);
    }
}

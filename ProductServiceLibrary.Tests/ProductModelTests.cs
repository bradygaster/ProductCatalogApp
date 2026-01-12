namespace ProductServiceLibrary.Tests;

public class ProductModelTests
{
    [Fact]
    public void Product_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Category = "Test Category",
            SKU = "TEST-001",
            StockQuantity = 10,
            ImageUrl = "https://example.com/image.jpg",
            IsActive = true,
            CreatedDate = DateTime.Now
        };

        // Assert
        Assert.Equal(1, product.Id);
        Assert.Equal("Test Product", product.Name);
        Assert.Equal("Test Description", product.Description);
        Assert.Equal(99.99m, product.Price);
        Assert.Equal("Test Category", product.Category);
        Assert.Equal("TEST-001", product.SKU);
        Assert.Equal(10, product.StockQuantity);
        Assert.Equal("https://example.com/image.jpg", product.ImageUrl);
        Assert.True(product.IsActive);
        Assert.NotEqual(default(DateTime), product.CreatedDate);
    }

    [Fact]
    public void Product_ShouldAllowOptionalLastModifiedDate()
    {
        // Arrange & Act
        var product = new Product
        {
            Id = 2,
            Name = "Another Product",
            LastModifiedDate = null
        };

        // Assert
        Assert.Null(product.LastModifiedDate);
    }

    [Fact]
    public void Product_ShouldSupportPriceCalculations()
    {
        // Arrange
        var product = new Product
        {
            Price = 100.00m,
            StockQuantity = 5
        };

        // Act
        var totalValue = product.Price * product.StockQuantity;

        // Assert
        Assert.Equal(500.00m, totalValue);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(10.50)]
    [InlineData(999.99)]
    [InlineData(10000.00)]
    public void Product_ShouldAcceptValidPrices(decimal price)
    {
        // Arrange & Act
        var product = new Product { Price = price };

        // Assert
        Assert.Equal(price, product.Price);
    }

    [Fact]
    public void Product_StockQuantity_ShouldSupportInventoryTracking()
    {
        // Arrange
        var product = new Product
        {
            StockQuantity = 100
        };

        // Act - Simulate selling 5 items
        product.StockQuantity -= 5;

        // Assert
        Assert.Equal(95, product.StockQuantity);
    }
}

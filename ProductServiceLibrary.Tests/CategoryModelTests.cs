namespace ProductServiceLibrary.Tests;

public class CategoryModelTests
{
    [Fact]
    public void Category_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var category = new Category
        {
            Id = 1,
            Name = "Electronics",
            Description = "Electronic devices and accessories"
        };

        // Assert
        Assert.Equal(1, category.Id);
        Assert.Equal("Electronics", category.Name);
        Assert.Equal("Electronic devices and accessories", category.Description);
    }

    [Fact]
    public void Category_ShouldSupportEmptyDescription()
    {
        // Arrange & Act
        var category = new Category
        {
            Id = 2,
            Name = "Books",
            Description = null
        };

        // Assert
        Assert.Equal(2, category.Id);
        Assert.Equal("Books", category.Name);
        Assert.Null(category.Description);
    }

    [Theory]
    [InlineData("Electronics", "Electronic devices and accessories")]
    [InlineData("Clothing", "Apparel and fashion items")]
    [InlineData("Books", "Books and publications")]
    public void Category_ShouldSupportMultipleCategories(string name, string description)
    {
        // Arrange & Act
        var category = new Category
        {
            Name = name,
            Description = description
        };

        // Assert
        Assert.Equal(name, category.Name);
        Assert.Equal(description, category.Description);
    }
}

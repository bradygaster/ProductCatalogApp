using ProductCatalogGrpcService.Models;

namespace ProductCatalogGrpcService.Repositories;

public class ProductRepository
{
    private static List<ProductData> _products = null!;
    private static List<CategoryData> _categories = null!;
    private static int _nextProductId;
    private static readonly object _lock = new object();

    static ProductRepository()
    {
        InitializeData();
    }

    private static void InitializeData()
    {
        _categories = new List<CategoryData>
        {
            new CategoryData { Id = 1, Name = "Electronics", Description = "Electronic devices and accessories" },
            new CategoryData { Id = 2, Name = "Clothing", Description = "Apparel and fashion items" },
            new CategoryData { Id = 3, Name = "Books", Description = "Books and publications" },
            new CategoryData { Id = 4, Name = "Home & Garden", Description = "Home improvement and garden supplies" },
            new CategoryData { Id = 5, Name = "Sports & Outdoors", Description = "Sports equipment and outdoor gear" },
            new CategoryData { Id = 6, Name = "Toys & Games", Description = "Toys, games, and puzzles" },
            new CategoryData { Id = 7, Name = "Food & Beverage", Description = "Food items and beverages" }
        };

        _products = new List<ProductData>
        {
            new ProductData
            {
                Id = 1,
                Name = "Wireless Bluetooth Headphones",
                Description = "Premium noise-cancelling wireless headphones with 30-hour battery life",
                Price = 149.99m,
                Category = "Electronics",
                Sku = "ELEC-WBH-001",
                StockQuantity = 45,
                ImageUrl = "https://example.com/images/headphones.jpg",
                IsActive = true,
                CreatedDate = DateTime.Now.AddMonths(-6),
                LastModifiedDate = DateTime.Now.AddDays(-10)
            },
            new ProductData
            {
                Id = 2,
                Name = "4K Smart TV 55 inch",
                Description = "Ultra HD Smart TV with HDR support and built-in streaming apps",
                Price = 599.99m,
                Category = "Electronics",
                Sku = "ELEC-TV-002",
                StockQuantity = 12,
                ImageUrl = "https://example.com/images/smart-tv.jpg",
                IsActive = true,
                CreatedDate = DateTime.Now.AddMonths(-4),
                LastModifiedDate = DateTime.Now.AddDays(-5)
            },
            new ProductData
            {
                Id = 3,
                Name = "Laptop Backpack",
                Description = "Water-resistant backpack with padded laptop compartment up to 17 inches",
                Price = 49.99m,
                Category = "Electronics",
                Sku = "ELEC-BAG-003",
                StockQuantity = 78,
                ImageUrl = "https://example.com/images/backpack.jpg",
                IsActive = true,
                CreatedDate = DateTime.Now.AddMonths(-8),
                LastModifiedDate = null
            }
        };

        _nextProductId = _products.Max(p => p.Id) + 1;
    }

    public List<ProductData> GetAllProducts()
    {
        lock (_lock)
        {
            return _products.Where(p => p.IsActive).ToList();
        }
    }

    public ProductData? GetProductById(int productId)
    {
        lock (_lock)
        {
            return _products.FirstOrDefault(p => p.Id == productId && p.IsActive);
        }
    }

    public List<ProductData> GetProductsByCategory(string category)
    {
        lock (_lock)
        {
            return _products.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase) && p.IsActive).ToList();
        }
    }

    public List<ProductData> SearchProducts(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return GetAllProducts();
        }

        lock (_lock)
        {
            return _products.Where(p => p.IsActive &&
                (p.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                 p.Description.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                 p.Category.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                 p.Sku.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)).ToList();
        }
    }

    public List<CategoryData> GetCategories()
    {
        lock (_lock)
        {
            return _categories.ToList();
        }
    }

    public ProductData CreateProduct(ProductData product)
    {
        if (product == null)
        {
            throw new ArgumentNullException(nameof(product));
        }

        lock (_lock)
        {
            product.Id = _nextProductId++;
            product.CreatedDate = DateTime.Now;
            product.IsActive = true;
            _products.Add(product);
            return product;
        }
    }

    public bool UpdateProduct(ProductData product)
    {
        if (product == null)
        {
            throw new ArgumentNullException(nameof(product));
        }

        lock (_lock)
        {
            var existingProduct = _products.FirstOrDefault(p => p.Id == product.Id);
            if (existingProduct == null)
            {
                return false;
            }

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.Category = product.Category;
            existingProduct.Sku = product.Sku;
            existingProduct.StockQuantity = product.StockQuantity;
            existingProduct.ImageUrl = product.ImageUrl;
            existingProduct.IsActive = product.IsActive;
            existingProduct.LastModifiedDate = DateTime.Now;

            return true;
        }
    }

    public bool DeleteProduct(int productId)
    {
        lock (_lock)
        {
            var product = _products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                return false;
            }

            product.IsActive = false;
            product.LastModifiedDate = DateTime.Now;
            return true;
        }
    }

    public List<ProductData> GetProductsByPriceRange(decimal minPrice, decimal maxPrice)
    {
        lock (_lock)
        {
            return _products.Where(p => p.IsActive && p.Price >= minPrice && p.Price <= maxPrice).ToList();
        }
    }
}

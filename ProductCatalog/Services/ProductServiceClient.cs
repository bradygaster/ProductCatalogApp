using System.Text.Json;

namespace ProductCatalog.Services;

public interface IProductServiceClient
{
    Task<List<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(int productId);
    Task<List<Product>> GetProductsByCategoryAsync(string category);
    Task<List<Product>> SearchProductsAsync(string searchTerm);
    Task<List<Category>> GetCategoriesAsync();
    Task<Product> CreateProductAsync(Product product);
    Task<bool> UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(int productId);
    Task<List<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
}

public class ProductServiceClient : IProductServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ProductServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        var response = await _httpClient.GetAsync("api/products");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Product>>(json, _jsonOptions) ?? new List<Product>();
    }

    public async Task<Product?> GetProductByIdAsync(int productId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/products/{productId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Product>(json, _jsonOptions);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public async Task<List<Product>> GetProductsByCategoryAsync(string category)
    {
        var response = await _httpClient.GetAsync($"api/products/category/{category}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Product>>(json, _jsonOptions) ?? new List<Product>();
    }

    public async Task<List<Product>> SearchProductsAsync(string searchTerm)
    {
        var response = await _httpClient.GetAsync($"api/products/search?searchTerm={Uri.EscapeDataString(searchTerm ?? "")}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Product>>(json, _jsonOptions) ?? new List<Product>();
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        var response = await _httpClient.GetAsync("api/categories");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Category>>(json, _jsonOptions) ?? new List<Category>();
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        var json = JsonSerializer.Serialize(product, _jsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/products", content);
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Product>(responseJson, _jsonOptions)!;
    }

    public async Task<bool> UpdateProductAsync(Product product)
    {
        var json = JsonSerializer.Serialize(product, _jsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"api/products/{product.Id}", content);
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<bool>(responseJson, _jsonOptions);
    }

    public async Task<bool> DeleteProductAsync(int productId)
    {
        var response = await _httpClient.DeleteAsync($"api/products/{productId}");
        response.EnsureSuccessStatusCode();
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<bool>(responseJson, _jsonOptions);
    }

    public async Task<List<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        var response = await _httpClient.GetAsync($"api/products/pricerange?minPrice={minPrice}&maxPrice={maxPrice}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<Product>>(json, _jsonOptions) ?? new List<Product>();
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

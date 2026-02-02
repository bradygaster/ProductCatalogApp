using ProductCatalog.Models;
using System.Text.Json;

namespace ProductCatalog.Services;

public class ProductService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ProductService(IConfiguration configuration, HttpClient httpClient)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["ProductServiceUrl"] ?? "https://localhost:5001";
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/products");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Product>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Product>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving products: {ex.Message}", ex);
        }
    }

    public async Task<Product?> GetProductByIdAsync(int productId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/products/{productId}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Product>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving product: {ex.Message}", ex);
        }
    }

    public async Task<List<Product>> GetProductsByCategoryAsync(string category)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/products/category/{category}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Product>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Product>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving products by category: {ex.Message}", ex);
        }
    }

    public async Task<List<Product>> SearchProductsAsync(string searchTerm)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/products/search?searchTerm={Uri.EscapeDataString(searchTerm)}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Product>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Product>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error searching products: {ex.Message}", ex);
        }
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/categories");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Category>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Category>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving categories: {ex.Message}", ex);
        }
    }
}

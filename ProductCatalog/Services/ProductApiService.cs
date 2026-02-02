using Newtonsoft.Json;
using ProductCatalog.Models;

namespace ProductCatalog.Services
{
    public class ProductApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ProductApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ProductApiUrl"] ?? "https://localhost:5001";
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/products");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Product>>(content) ?? new List<Product>();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/products/{id}");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Product>(content);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Product>> GetProductsByCategoryAsync(string category)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/products/category/{category}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Product>>(content) ?? new List<Product>();
        }

        public async Task<List<Product>> SearchProductsAsync(string searchTerm)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/products/search?searchTerm={Uri.EscapeDataString(searchTerm)}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Product>>(content) ?? new List<Product>();
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/categories");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Category>>(content) ?? new List<Category>();
        }
    }
}

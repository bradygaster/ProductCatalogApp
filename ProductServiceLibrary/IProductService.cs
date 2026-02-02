using System.Collections.Generic;

namespace ProductServiceLibrary
{
    public interface IProductService
    {
        List<Product> GetAllProducts();
        Product GetProductById(int productId);
        List<Product> GetProductsByCategory(string category);
        List<Product> SearchProducts(string searchTerm);
        List<Category> GetCategories();
        Product CreateProduct(Product product);
        bool UpdateProduct(Product product);
        bool DeleteProduct(int productId);
        List<Product> GetProductsByPriceRange(decimal minPrice, decimal maxPrice);
    }
}

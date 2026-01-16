using System.Collections.Generic;
using CoreWCF;

namespace ProductServiceLibrary
{
    [ServiceContract]
    public interface IProductService
    {
        [OperationContract]
        List<Product> GetAllProducts();

        [OperationContract]
        Product GetProductById(int productId);

        [OperationContract]
        List<Product> GetProductsByCategory(string category);

        [OperationContract]
        List<Product> SearchProducts(string searchTerm);

        [OperationContract]
        List<Category> GetCategories();

        [OperationContract]
        Product CreateProduct(Product product);

        [OperationContract]
        bool UpdateProduct(Product product);

        [OperationContract]
        bool DeleteProduct(int productId);

        [OperationContract]
        List<Product> GetProductsByPriceRange(decimal minPrice, decimal maxPrice);
    }
}

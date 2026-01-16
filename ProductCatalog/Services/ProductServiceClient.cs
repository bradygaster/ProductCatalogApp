using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using ProductServiceLibrary;

namespace ProductCatalog.Services
{
    public class ProductServiceClient : IDisposable
    {
        private readonly ChannelFactory<IProductService> _channelFactory;
        private readonly IProductService _channel;

        public ProductServiceClient()
        {
            var binding = new BasicHttpBinding();
            var endpoint = new EndpointAddress("http://localhost:5000/ProductService");
            _channelFactory = new ChannelFactory<IProductService>(binding, endpoint);
            _channel = _channelFactory.CreateChannel();
        }

        public List<Product> GetAllProducts()
        {
            return _channel.GetAllProducts();
        }

        public Product GetProductById(int productId)
        {
            return _channel.GetProductById(productId);
        }

        public List<Product> GetProductsByCategory(string category)
        {
            return _channel.GetProductsByCategory(category);
        }

        public List<Product> SearchProducts(string searchTerm)
        {
            return _channel.SearchProducts(searchTerm);
        }

        public List<Category> GetCategories()
        {
            return _channel.GetCategories();
        }

        public Product CreateProduct(Product product)
        {
            return _channel.CreateProduct(product);
        }

        public bool UpdateProduct(Product product)
        {
            return _channel.UpdateProduct(product);
        }

        public bool DeleteProduct(int productId)
        {
            return _channel.DeleteProduct(productId);
        }

        public List<Product> GetProductsByPriceRange(decimal minPrice, decimal maxPrice)
        {
            return _channel.GetProductsByPriceRange(minPrice, maxPrice);
        }

        public void Dispose()
        {
            try
            {
                if (_channel is IClientChannel clientChannel)
                {
                    if (clientChannel.State == CommunicationState.Faulted)
                        clientChannel.Abort();
                    else
                        clientChannel.Close();
                }
                
                _channelFactory?.Close();
            }
            catch
            {
                _channelFactory?.Abort();
            }
        }
    }
}

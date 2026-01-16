using System;
using System.Collections.Generic;
using CoreWCF;

namespace ProductServiceLibrary
{
    public class ProductService : IProductService
    {
        private readonly ProductRepository _repository;

        public ProductService()
        {
            _repository = new ProductRepository();
        }

        public List<Product> GetAllProducts()
        {
            try
            {
                return _repository.GetAllProducts();
            }
            catch (Exception ex)
            {
                throw new FaultException($"Error retrieving all products: {ex.Message}");
            }
        }

        public Product GetProductById(int productId)
        {
            try
            {
                var product = _repository.GetProductById(productId);
                if (product == null)
                {
                    throw new FaultException($"Product with ID {productId} not found");
                }
                return product;
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FaultException($"Error retrieving product: {ex.Message}");
            }
        }

        public List<Product> GetProductsByCategory(string category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category))
                {
                    throw new FaultException("Category parameter cannot be null or empty");
                }
                return _repository.GetProductsByCategory(category);
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FaultException($"Error retrieving products by category: {ex.Message}");
            }
        }

        public List<Product> SearchProducts(string searchTerm)
        {
            try
            {
                return _repository.SearchProducts(searchTerm);
            }
            catch (Exception ex)
            {
                throw new FaultException($"Error searching products: {ex.Message}");
            }
        }

        public List<Category> GetCategories()
        {
            try
            {
                return _repository.GetCategories();
            }
            catch (Exception ex)
            {
                throw new FaultException($"Error retrieving categories: {ex.Message}");
            }
        }

        public Product CreateProduct(Product product)
        {
            try
            {
                if (product == null)
                {
                    throw new FaultException("Product parameter cannot be null");
                }

                if (string.IsNullOrWhiteSpace(product.Name))
                {
                    throw new FaultException("Product name is required");
                }

                if (product.Price < 0)
                {
                    throw new FaultException("Product price must be non-negative");
                }

                return _repository.CreateProduct(product);
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FaultException($"Error creating product: {ex.Message}");
            }
        }

        public bool UpdateProduct(Product product)
        {
            try
            {
                if (product == null)
                {
                    throw new FaultException("Product parameter cannot be null");
                }

                if (string.IsNullOrWhiteSpace(product.Name))
                {
                    throw new FaultException("Product name is required");
                }

                if (product.Price < 0)
                {
                    throw new FaultException("Product price must be non-negative");
                }

                bool result = _repository.UpdateProduct(product);
                if (!result)
                {
                    throw new FaultException($"Product with ID {product.Id} not found");
                }

                return result;
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FaultException($"Error updating product: {ex.Message}");
            }
        }

        public bool DeleteProduct(int productId)
        {
            try
            {
                bool result = _repository.DeleteProduct(productId);
                if (!result)
                {
                    throw new FaultException($"Product with ID {productId} not found");
                }
                return result;
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FaultException($"Error deleting product: {ex.Message}");
            }
        }

        public List<Product> GetProductsByPriceRange(decimal minPrice, decimal maxPrice)
        {
            try
            {
                if (minPrice < 0 || maxPrice < 0)
                {
                    throw new FaultException("Price range values must be non-negative");
                }

                if (minPrice > maxPrice)
                {
                    throw new FaultException("Minimum price cannot be greater than maximum price");
                }

                return _repository.GetProductsByPriceRange(minPrice, maxPrice);
            }
            catch (FaultException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FaultException($"Error retrieving products by price range: {ex.Message}");
            }
        }
    }
}

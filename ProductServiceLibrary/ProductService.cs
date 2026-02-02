using System;
using System.Collections.Generic;

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
                throw new InvalidOperationException($"Error retrieving all products: {ex.Message}", ex);
            }
        }

        public Product GetProductById(int productId)
        {
            try
            {
                var product = _repository.GetProductById(productId);
                if (product == null)
                {
                    throw new InvalidOperationException($"Product with ID {productId} not found");
                }
                return product;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving product: {ex.Message}", ex);
            }
        }

        public List<Product> GetProductsByCategory(string category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category))
                {
                    throw new ArgumentException("Category parameter cannot be null or empty", nameof(category));
                }
                return _repository.GetProductsByCategory(category);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving products by category: {ex.Message}", ex);
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
                throw new InvalidOperationException($"Error searching products: {ex.Message}", ex);
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
                throw new InvalidOperationException($"Error retrieving categories: {ex.Message}", ex);
            }
        }

        public Product CreateProduct(Product product)
        {
            try
            {
                if (product == null)
                {
                    throw new ArgumentNullException(nameof(product), "Product parameter cannot be null");
                }

                if (string.IsNullOrWhiteSpace(product.Name))
                {
                    throw new ArgumentException("Product name is required", nameof(product));
                }

                if (product.Price < 0)
                {
                    throw new ArgumentException("Product price must be non-negative", nameof(product));
                }

                return _repository.CreateProduct(product);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating product: {ex.Message}", ex);
            }
        }

        public bool UpdateProduct(Product product)
        {
            try
            {
                if (product == null)
                {
                    throw new ArgumentNullException(nameof(product), "Product parameter cannot be null");
                }

                if (string.IsNullOrWhiteSpace(product.Name))
                {
                    throw new ArgumentException("Product name is required", nameof(product));
                }

                if (product.Price < 0)
                {
                    throw new ArgumentException("Product price must be non-negative", nameof(product));
                }

                bool result = _repository.UpdateProduct(product);
                if (!result)
                {
                    throw new InvalidOperationException($"Product with ID {product.Id} not found");
                }

                return result;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating product: {ex.Message}", ex);
            }
        }

        public bool DeleteProduct(int productId)
        {
            try
            {
                bool result = _repository.DeleteProduct(productId);
                if (!result)
                {
                    throw new InvalidOperationException($"Product with ID {productId} not found");
                }
                return result;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deleting product: {ex.Message}", ex);
            }
        }

        public List<Product> GetProductsByPriceRange(decimal minPrice, decimal maxPrice)
        {
            try
            {
                if (minPrice < 0 || maxPrice < 0)
                {
                    throw new ArgumentException("Price range values must be non-negative");
                }

                if (minPrice > maxPrice)
                {
                    throw new ArgumentException("Minimum price cannot be greater than maximum price");
                }

                return _repository.GetProductsByPriceRange(minPrice, maxPrice);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving products by price range: {ex.Message}", ex);
            }
        }
    }
}

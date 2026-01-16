using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using ProductCatalogGrpcService.Repositories;
using ProductCatalogGrpcService.Models;

namespace ProductCatalogGrpcService.Services;

public class ProductGrpcService : ProductService.ProductServiceBase
{
    private readonly ProductRepository _repository;
    private readonly ILogger<ProductGrpcService> _logger;

    public ProductGrpcService(ProductRepository repository, ILogger<ProductGrpcService> logger)
    {
        _logger = logger;
        _repository = repository;
    }

    public override Task<ProductListResponse> GetAllProducts(EmptyRequest request, ServerCallContext context)
    {
        try
        {
            var products = _repository.GetAllProducts();
            var response = new ProductListResponse();
            response.Products.AddRange(products.Select(MapToProto));
            return Task.FromResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all products");
            return Task.FromResult(new ProductListResponse { ErrorMessage = $"Error retrieving all products: {ex.Message}" });
        }
    }

    public override Task<ProductResponse> GetProductById(ProductByIdRequest request, ServerCallContext context)
    {
        try
        {
            var product = _repository.GetProductById(request.ProductId);
            if (product == null)
            {
                return Task.FromResult(new ProductResponse 
                { 
                    ErrorMessage = $"Product with ID {request.ProductId} not found" 
                });
            }
            
            return Task.FromResult(new ProductResponse { Product = MapToProto(product) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product by ID");
            return Task.FromResult(new ProductResponse { ErrorMessage = $"Error retrieving product: {ex.Message}" });
        }
    }

    public override Task<ProductListResponse> GetProductsByCategory(ProductsByCategoryRequest request, ServerCallContext context)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Category))
            {
                return Task.FromResult(new ProductListResponse 
                { 
                    ErrorMessage = "Category parameter cannot be null or empty" 
                });
            }

            var products = _repository.GetProductsByCategory(request.Category);
            var response = new ProductListResponse();
            response.Products.AddRange(products.Select(MapToProto));
            return Task.FromResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products by category");
            return Task.FromResult(new ProductListResponse { ErrorMessage = $"Error retrieving products by category: {ex.Message}" });
        }
    }

    public override Task<ProductListResponse> SearchProducts(SearchProductsRequest request, ServerCallContext context)
    {
        try
        {
            var products = _repository.SearchProducts(request.SearchTerm);
            var response = new ProductListResponse();
            response.Products.AddRange(products.Select(MapToProto));
            return Task.FromResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products");
            return Task.FromResult(new ProductListResponse { ErrorMessage = $"Error searching products: {ex.Message}" });
        }
    }

    public override Task<CategoryListResponse> GetCategories(EmptyRequest request, ServerCallContext context)
    {
        try
        {
            var categories = _repository.GetCategories();
            var response = new CategoryListResponse();
            response.Categories.AddRange(categories.Select(MapToProto));
            return Task.FromResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return Task.FromResult(new CategoryListResponse { ErrorMessage = $"Error retrieving categories: {ex.Message}" });
        }
    }

    public override Task<ProductResponse> CreateProduct(CreateProductRequest request, ServerCallContext context)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Task.FromResult(new ProductResponse { ErrorMessage = "Product name is required" });
            }

            if (request.Price < 0)
            {
                return Task.FromResult(new ProductResponse { ErrorMessage = "Product price must be non-negative" });
            }

            var productData = new ProductData
            {
                Name = request.Name,
                Description = request.Description,
                Price = (decimal)request.Price,
                Category = request.Category,
                Sku = request.Sku,
                StockQuantity = request.StockQuantity,
                ImageUrl = request.ImageUrl
            };

            var createdProduct = _repository.CreateProduct(productData);
            return Task.FromResult(new ProductResponse { Product = MapToProto(createdProduct) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return Task.FromResult(new ProductResponse { ErrorMessage = $"Error creating product: {ex.Message}" });
        }
    }

    public override Task<UpdateProductResponse> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Task.FromResult(new UpdateProductResponse 
                { 
                    Success = false, 
                    ErrorMessage = "Product name is required" 
                });
            }

            if (request.Price < 0)
            {
                return Task.FromResult(new UpdateProductResponse 
                { 
                    Success = false, 
                    ErrorMessage = "Product price must be non-negative" 
                });
            }

            var productData = new ProductData
            {
                Id = request.Id,
                Name = request.Name,
                Description = request.Description,
                Price = (decimal)request.Price,
                Category = request.Category,
                Sku = request.Sku,
                StockQuantity = request.StockQuantity,
                ImageUrl = request.ImageUrl,
                IsActive = request.IsActive
            };

            var result = _repository.UpdateProduct(productData);
            if (!result)
            {
                return Task.FromResult(new UpdateProductResponse 
                { 
                    Success = false, 
                    ErrorMessage = $"Product with ID {request.Id} not found" 
                });
            }

            return Task.FromResult(new UpdateProductResponse { Success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product");
            return Task.FromResult(new UpdateProductResponse 
            { 
                Success = false, 
                ErrorMessage = $"Error updating product: {ex.Message}" 
            });
        }
    }

    public override Task<DeleteProductResponse> DeleteProduct(DeleteProductRequest request, ServerCallContext context)
    {
        try
        {
            var result = _repository.DeleteProduct(request.ProductId);
            if (!result)
            {
                return Task.FromResult(new DeleteProductResponse 
                { 
                    Success = false, 
                    ErrorMessage = $"Product with ID {request.ProductId} not found" 
                });
            }

            return Task.FromResult(new DeleteProductResponse { Success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product");
            return Task.FromResult(new DeleteProductResponse 
            { 
                Success = false, 
                ErrorMessage = $"Error deleting product: {ex.Message}" 
            });
        }
    }

    public override Task<ProductListResponse> GetProductsByPriceRange(ProductsByPriceRangeRequest request, ServerCallContext context)
    {
        try
        {
            if (request.MinPrice < 0 || request.MaxPrice < 0)
            {
                return Task.FromResult(new ProductListResponse 
                { 
                    ErrorMessage = "Price range values must be non-negative" 
                });
            }

            if (request.MinPrice > request.MaxPrice)
            {
                return Task.FromResult(new ProductListResponse 
                { 
                    ErrorMessage = "Minimum price cannot be greater than maximum price" 
                });
            }

            var products = _repository.GetProductsByPriceRange((decimal)request.MinPrice, (decimal)request.MaxPrice);
            var response = new ProductListResponse();
            response.Products.AddRange(products.Select(MapToProto));
            return Task.FromResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products by price range");
            return Task.FromResult(new ProductListResponse { ErrorMessage = $"Error retrieving products by price range: {ex.Message}" });
        }
    }

    // Mapping helpers
    private Product MapToProto(ProductData productData)
    {
        var product = new Product
        {
            Id = productData.Id,
            Name = productData.Name,
            Description = productData.Description,
            Price = (double)productData.Price,
            Category = productData.Category,
            Sku = productData.Sku,
            StockQuantity = productData.StockQuantity,
            ImageUrl = productData.ImageUrl,
            IsActive = productData.IsActive,
            CreatedDate = Timestamp.FromDateTime(DateTime.SpecifyKind(productData.CreatedDate, DateTimeKind.Utc))
        };

        if (productData.LastModifiedDate.HasValue)
        {
            product.LastModifiedDate = Timestamp.FromDateTime(DateTime.SpecifyKind(productData.LastModifiedDate.Value, DateTimeKind.Utc));
        }

        return product;
    }

    private Category MapToProto(CategoryData categoryData)
    {
        return new Category
        {
            Id = categoryData.Id,
            Name = categoryData.Name,
            Description = categoryData.Description
        };
    }
}

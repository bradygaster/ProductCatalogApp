using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using ProductServiceGrpc.Models;

namespace ProductServiceGrpc.Services;

public class ProductGrpcService : ProductService.ProductServiceBase
{
    private readonly ProductRepository _repository;
    private readonly ILogger<ProductGrpcService> _logger;

    public ProductGrpcService(ILogger<ProductGrpcService> logger)
    {
        _logger = logger;
        _repository = new ProductRepository();
    }

    public override Task<GetAllProductsResponse> GetAllProducts(
        GetAllProductsRequest request, 
        ServerCallContext context)
    {
        try
        {
            var products = _repository.GetAllProducts();
            var response = new GetAllProductsResponse();
            
            foreach (var product in products)
            {
                response.Products.Add(MapToGrpcProduct(product));
            }
            
            return Task.FromResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all products");
            throw new RpcException(new Status(StatusCode.Internal, $"Error retrieving all products: {ex.Message}"));
        }
    }

    public override Task<ProductResponse> GetProductById(
        GetProductByIdRequest request, 
        ServerCallContext context)
    {
        try
        {
            var product = _repository.GetProductById(request.ProductId);
            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID {request.ProductId} not found"));
            }
            
            var response = new ProductResponse
            {
                Product = MapToGrpcProduct(product)
            };
            
            return Task.FromResult(response);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product by ID {ProductId}", request.ProductId);
            throw new RpcException(new Status(StatusCode.Internal, $"Error retrieving product: {ex.Message}"));
        }
    }

    public override Task<GetProductsResponse> GetProductsByCategory(
        GetProductsByCategoryRequest request, 
        ServerCallContext context)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Category))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Category parameter cannot be null or empty"));
            }
            
            var products = _repository.GetProductsByCategory(request.Category);
            var response = new GetProductsResponse();
            
            foreach (var product in products)
            {
                response.Products.Add(MapToGrpcProduct(product));
            }
            
            return Task.FromResult(response);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products by category {Category}", request.Category);
            throw new RpcException(new Status(StatusCode.Internal, $"Error retrieving products by category: {ex.Message}"));
        }
    }

    public override Task<GetProductsResponse> SearchProducts(
        SearchProductsRequest request, 
        ServerCallContext context)
    {
        try
        {
            var products = _repository.SearchProducts(request.SearchTerm);
            var response = new GetProductsResponse();
            
            foreach (var product in products)
            {
                response.Products.Add(MapToGrpcProduct(product));
            }
            
            return Task.FromResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products with term {SearchTerm}", request.SearchTerm);
            throw new RpcException(new Status(StatusCode.Internal, $"Error searching products: {ex.Message}"));
        }
    }

    public override Task<GetCategoriesResponse> GetCategories(
        GetCategoriesRequest request, 
        ServerCallContext context)
    {
        try
        {
            var categories = _repository.GetCategories();
            var response = new GetCategoriesResponse();
            
            foreach (var category in categories)
            {
                response.Categories.Add(new Category
                {
                    Id = category.Id,
                    Name = category.Name ?? string.Empty,
                    Description = category.Description ?? string.Empty
                });
            }
            
            return Task.FromResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            throw new RpcException(new Status(StatusCode.Internal, $"Error retrieving categories: {ex.Message}"));
        }
    }

    public override Task<ProductResponse> CreateProduct(
        CreateProductRequest request, 
        ServerCallContext context)
    {
        try
        {
            if (request.Product == null)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Product parameter cannot be null"));
            }

            if (string.IsNullOrWhiteSpace(request.Product.Name))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Product name is required"));
            }

            if (request.Product.Price < 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Product price must be non-negative"));
            }

            var product = MapFromGrpcProduct(request.Product);
            var createdProduct = _repository.CreateProduct(product);
            
            var response = new ProductResponse
            {
                Product = MapToGrpcProduct(createdProduct)
            };
            
            return Task.FromResult(response);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            throw new RpcException(new Status(StatusCode.Internal, $"Error creating product: {ex.Message}"));
        }
    }

    public override Task<UpdateProductResponse> UpdateProduct(
        UpdateProductRequest request, 
        ServerCallContext context)
    {
        try
        {
            if (request.Product == null)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Product parameter cannot be null"));
            }

            if (string.IsNullOrWhiteSpace(request.Product.Name))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Product name is required"));
            }

            if (request.Product.Price < 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Product price must be non-negative"));
            }

            var product = MapFromGrpcProduct(request.Product);
            var result = _repository.UpdateProduct(product);
            
            if (!result)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID {request.Product.Id} not found"));
            }

            var response = new UpdateProductResponse
            {
                Success = result
            };
            
            return Task.FromResult(response);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product");
            throw new RpcException(new Status(StatusCode.Internal, $"Error updating product: {ex.Message}"));
        }
    }

    public override Task<DeleteProductResponse> DeleteProduct(
        DeleteProductRequest request, 
        ServerCallContext context)
    {
        try
        {
            var result = _repository.DeleteProduct(request.ProductId);
            
            if (!result)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID {request.ProductId} not found"));
            }

            var response = new DeleteProductResponse
            {
                Success = result
            };
            
            return Task.FromResult(response);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", request.ProductId);
            throw new RpcException(new Status(StatusCode.Internal, $"Error deleting product: {ex.Message}"));
        }
    }

    public override Task<GetProductsResponse> GetProductsByPriceRange(
        GetProductsByPriceRangeRequest request, 
        ServerCallContext context)
    {
        try
        {
            if (request.MinPrice < 0 || request.MaxPrice < 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Price range values must be non-negative"));
            }

            if (request.MinPrice > request.MaxPrice)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Minimum price cannot be greater than maximum price"));
            }

            var products = _repository.GetProductsByPriceRange((decimal)request.MinPrice, (decimal)request.MaxPrice);
            var response = new GetProductsResponse();
            
            foreach (var product in products)
            {
                response.Products.Add(MapToGrpcProduct(product));
            }
            
            return Task.FromResult(response);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products by price range");
            throw new RpcException(new Status(StatusCode.Internal, $"Error retrieving products by price range: {ex.Message}"));
        }
    }

    private Product MapToGrpcProduct(Models.Product product)
    {
        var grpcProduct = new Product
        {
            Id = product.Id,
            Name = product.Name ?? string.Empty,
            Description = product.Description ?? string.Empty,
            Price = (double)product.Price,
            Category = product.Category ?? string.Empty,
            Sku = product.SKU ?? string.Empty,
            StockQuantity = product.StockQuantity,
            ImageUrl = product.ImageUrl ?? string.Empty,
            IsActive = product.IsActive,
            CreatedDate = Timestamp.FromDateTime(DateTime.SpecifyKind(product.CreatedDate, DateTimeKind.Utc))
        };

        if (product.LastModifiedDate.HasValue)
        {
            grpcProduct.LastModifiedDate = Timestamp.FromDateTime(DateTime.SpecifyKind(product.LastModifiedDate.Value, DateTimeKind.Utc));
        }

        return grpcProduct;
    }

    private Models.Product MapFromGrpcProduct(Product grpcProduct)
    {
        var product = new Models.Product
        {
            Id = grpcProduct.Id,
            Name = grpcProduct.Name,
            Description = grpcProduct.Description,
            Price = (decimal)grpcProduct.Price,
            Category = grpcProduct.Category,
            SKU = grpcProduct.Sku,
            StockQuantity = grpcProduct.StockQuantity,
            ImageUrl = grpcProduct.ImageUrl,
            IsActive = grpcProduct.IsActive,
            CreatedDate = grpcProduct.CreatedDate?.ToDateTime() ?? DateTime.UtcNow
        };

        if (grpcProduct.LastModifiedDate != null)
        {
            product.LastModifiedDate = grpcProduct.LastModifiedDate.ToDateTime();
        }

        return product;
    }
}

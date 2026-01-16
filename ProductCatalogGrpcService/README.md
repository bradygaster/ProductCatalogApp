# Product Catalog gRPC Service

This is a modernized gRPC implementation of the Product Catalog service, migrated from the legacy WCF service.

## Overview

The Product Catalog gRPC Service provides a modern, high-performance API for managing products and categories using Google's gRPC framework. This service replaces the legacy WCF implementation with a more efficient, cross-platform solution.

## Features

- **Product Management**: Create, read, update, and delete products
- **Category Management**: Retrieve product categories
- **Search Functionality**: Search products by name, description, category, or SKU
- **Price Range Filtering**: Get products within a specific price range
- **Category Filtering**: Get products by category
- **High Performance**: Built on gRPC for efficient binary serialization and HTTP/2 support
- **Cross-Platform**: Runs on .NET 10.0 with support for Windows, Linux, and macOS

## Technology Stack

- **.NET 10.0**: Modern .NET platform
- **gRPC**: High-performance RPC framework
- **Protocol Buffers**: Efficient data serialization
- **ASP.NET Core**: Web hosting framework

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- gRPC-compatible client (e.g., grpcurl, Postman, or custom client)

### Running the Service

```bash
cd ProductCatalogGrpcService
dotnet run
```

The service will start on:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

### Service Definition

The service exposes the following gRPC methods:

#### Product Operations

- `GetAllProducts`: Retrieve all active products
- `GetProductById`: Get a specific product by ID
- `GetProductsByCategory`: Filter products by category name
- `SearchProducts`: Search products by keyword
- `GetProductsByPriceRange`: Filter products by price range
- `CreateProduct`: Add a new product
- `UpdateProduct`: Modify an existing product
- `DeleteProduct`: Soft-delete a product (marks as inactive)

#### Category Operations

- `GetCategories`: Retrieve all available categories

### Protocol Buffers Definition

The service contract is defined in `Protos/product.proto`. Key message types include:

- `Product`: Product details with ID, name, description, price, category, SKU, stock quantity, image URL, and timestamps
- `Category`: Category information with ID, name, and description
- Various request/response messages for each operation

### Example Usage with grpcurl

#### List all products
```bash
grpcurl -plaintext localhost:5000 product.ProductService/GetAllProducts
```

#### Get product by ID
```bash
grpcurl -plaintext -d '{"product_id": 1}' localhost:5000 product.ProductService/GetProductById
```

#### Search products
```bash
grpcurl -plaintext -d '{"search_term": "headphones"}' localhost:5000 product.ProductService/SearchProducts
```

#### Get categories
```bash
grpcurl -plaintext localhost:5000 product.ProductService/GetCategories
```

### Creating a .NET Client

To consume this service from a .NET application:

1. Add the gRPC client package:
```bash
dotnet add package Grpc.Net.Client
dotnet add package Google.Protobuf
dotnet add package Grpc.Tools
```

2. Reference the `product.proto` file in your client project

3. Use the generated client:
```csharp
using var channel = GrpcChannel.ForAddress("https://localhost:5001");
var client = new ProductService.ProductServiceClient(channel);

var response = await client.GetAllProductsAsync(new EmptyRequest());
foreach (var product in response.Products)
{
    Console.WriteLine($"{product.Name} - ${product.Price}");
}
```

## Migration from WCF

This gRPC service provides equivalent functionality to the original WCF service with the following improvements:

### Advantages over WCF

1. **Better Performance**: Binary serialization and HTTP/2 support
2. **Cross-Platform**: Works on Windows, Linux, and macOS
3. **Modern Tooling**: Better IDE support and tooling
4. **Bi-directional Streaming**: Support for real-time scenarios (if needed in the future)
5. **Smaller Payload**: Protocol Buffers are more compact than SOAP XML
6. **Language Interoperability**: Easy client generation for multiple languages

### Mapping from WCF to gRPC

| WCF Service Operation | gRPC Method | Notes |
|----------------------|-------------|--------|
| GetAllProducts | GetAllProducts | Direct mapping |
| GetProductById | GetProductById | Direct mapping |
| GetProductsByCategory | GetProductsByCategory | Direct mapping |
| SearchProducts | SearchProducts | Direct mapping |
| GetCategories | GetCategories | Direct mapping |
| CreateProduct | CreateProduct | Direct mapping |
| UpdateProduct | UpdateProduct | Direct mapping |
| DeleteProduct | DeleteProduct | Soft delete (sets IsActive = false) |
| GetProductsByPriceRange | GetProductsByPriceRange | Direct mapping |

### Error Handling

Instead of WCF's `FaultException`, this service returns error messages in the response objects:
- `ProductResponse.ErrorMessage`
- `ProductListResponse.ErrorMessage`
- `CategoryListResponse.ErrorMessage`
- `UpdateProductResponse.ErrorMessage` + `Success` flag
- `DeleteProductResponse.ErrorMessage` + `Success` flag

## Development

### Project Structure

```
ProductCatalogGrpcService/
├── Models/              # Internal data models
│   ├── ProductData.cs
│   └── CategoryData.cs
├── Protos/              # Protocol Buffer definitions
│   ├── greet.proto
│   └── product.proto
├── Repositories/        # Data access layer
│   └── ProductRepository.cs
├── Services/            # gRPC service implementations
│   ├── GreeterService.cs
│   └── ProductGrpcService.cs
├── Program.cs           # Application entry point
└── appsettings.json     # Configuration
```

### Building the Project

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

## Configuration

The service can be configured through `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Customizing Ports

Modify `Properties/launchSettings.json` or use command-line arguments:

```bash
dotnet run --urls "http://localhost:8080;https://localhost:8081"
```

## Security Considerations

For production deployment:

1. **Enable HTTPS**: Always use TLS/SSL in production
2. **Authentication**: Implement token-based authentication (JWT)
3. **Authorization**: Add role-based access control
4. **Rate Limiting**: Implement rate limiting to prevent abuse
5. **Input Validation**: Already implemented for basic scenarios

## Future Enhancements

- Add authentication and authorization
- Implement caching for improved performance
- Add real-time updates using gRPC streaming
- Integrate with a persistent database (currently uses in-memory storage)
- Add comprehensive unit and integration tests
- Implement health checks
- Add OpenTelemetry for observability

## Resources

- [gRPC Documentation](https://grpc.io/docs/)
- [gRPC for .NET](https://docs.microsoft.com/en-us/aspnet/core/grpc/)
- [Protocol Buffers](https://developers.google.com/protocol-buffers)

## License

[Add your license information here]

## Contributors

[Add contributors information here]

# Product Catalog Application - .NET 10

This application has been modernized from .NET Framework 4.8.1 to .NET 10, converting WCF services to Web API and migrating from MSMQ to Azure Service Bus.

## Architecture

### ProductServiceLibrary
A RESTful Web API service built with ASP.NET Core that provides product management functionality.

**Endpoints:**
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/category/{category}` - Get products by category
- `GET /api/products/search?searchTerm={term}` - Search products
- `GET /api/products/pricerange?minPrice={min}&maxPrice={max}` - Get products by price range
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product
- `GET /api/categories` - Get all categories

Swagger UI is available at: `http://localhost:5000/swagger`

### ProductCatalog
An ASP.NET Core MVC web application that consumes the ProductServiceLibrary API and allows users to browse products, add them to a shopping cart, and submit orders.

**Features:**
- Browse product catalog
- Add products to cart
- Update cart quantities
- Submit orders (sent to Azure Service Bus queue)
- Session-based shopping cart

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Azure Service Bus namespace (for order queue functionality)

## Configuration

### ProductServiceLibrary

Located at: `ProductServiceLibrary/appsettings.json`

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

Default port: 5000 (configurable in `launchSettings.json`)

### ProductCatalog

Located at: `ProductCatalog/appsettings.json`

```json
{
  "ProductServiceUrl": "http://localhost:5000",
  "ServiceBusConnectionString": "<your-connection-string>",
  "OrderQueueName": "productcatalogorders"
}
```

**Required Configuration:**
- `ProductServiceUrl`: URL of the ProductServiceLibrary API
- `ServiceBusConnectionString`: Azure Service Bus connection string
- `OrderQueueName`: Name of the Service Bus queue for orders

### Setting up Azure Service Bus

1. Create an Azure Service Bus namespace in the Azure Portal
2. Create a queue named `productcatalogorders` (or your configured name)
3. Get the connection string from "Shared access policies" → "RootManageSharedAccessKey"
4. Update the `ServiceBusConnectionString` in `appsettings.json`

**For local development without Azure:**
You can use [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite) or comment out the order submission functionality temporarily.

## Running the Application

### Option 1: Run both projects separately

**Terminal 1 - Start ProductServiceLibrary API:**
```bash
cd ProductServiceLibrary
dotnet run
```

**Terminal 2 - Start ProductCatalog Web App:**
```bash
cd ProductCatalog
dotnet run
```

Navigate to the ProductCatalog URL (typically `http://localhost:5001` or `https://localhost:7001`)

### Option 2: Build and run from solution root

```bash
# Build both projects
dotnet build

# Run ProductServiceLibrary
cd ProductServiceLibrary
dotnet run &

# Run ProductCatalog
cd ../ProductCatalog
dotnet run
```

## Key Changes from .NET Framework 4.8.1

### ProductServiceLibrary
- ✅ Converted from WCF service library to ASP.NET Core Web API
- ✅ Removed `[ServiceContract]`, `[OperationContract]`, `[DataContract]`, `[DataMember]` attributes
- ✅ Replaced `FaultException` with standard .NET exceptions (`ArgumentException`, `InvalidOperationException`)
- ✅ Added Swagger/OpenAPI documentation
- ✅ Implemented RESTful API endpoints
- ✅ Added CORS support for cross-origin requests

### ProductCatalog
- ✅ Converted from ASP.NET MVC 5 to ASP.NET Core MVC
- ✅ Migrated from `Web.config` to `appsettings.json`
- ✅ Replaced `Global.asax.cs` with `Program.cs`
- ✅ Updated controllers to use async/await patterns
- ✅ Replaced WCF client with `HttpClient`-based `ProductServiceClient`
- ✅ Updated views to use ASP.NET Core Tag Helpers (`asp-controller`, `asp-action`)
- ✅ Migrated session handling to ASP.NET Core session middleware
- ✅ Replaced `System.Messaging` (MSMQ) with `Azure.Messaging.ServiceBus`
- ✅ Updated order serialization to use `System.Text.Json`

### MSMQ to Azure Service Bus Migration
- ✅ Replaced synchronous `SendOrder()` with async `SendOrderAsync()`
- ✅ Changed configuration from `OrderQueuePath` to `ServiceBusConnectionString` and `OrderQueueName`
- ✅ Implemented proper message handling with Service Bus SDK
- ✅ Added JSON serialization for order messages

## Testing

### Build both projects
```bash
dotnet build
```

Expected output:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Test ProductServiceLibrary API
```bash
cd ProductServiceLibrary
dotnet run
```

Then visit `http://localhost:5000/swagger` to test the API endpoints.

Or use curl:
```bash
curl http://localhost:5000/api/products
curl http://localhost:5000/api/categories
```

### Test ProductCatalog Web App
Ensure ProductServiceLibrary is running, then:
```bash
cd ProductCatalog
dotnet run
```

Navigate to the displayed URL (e.g., `https://localhost:7001`) and test:
1. Browse products on home page
2. Add products to cart
3. View cart
4. Update quantities
5. Submit order (requires Azure Service Bus configuration)

## Troubleshooting

### "ServiceBusConnectionString not configured" error
Make sure you've set the `ServiceBusConnectionString` in `ProductCatalog/appsettings.json` to a valid Azure Service Bus connection string.

### ProductCatalog can't connect to ProductServiceLibrary
1. Verify ProductServiceLibrary is running
2. Check the `ProductServiceUrl` in `ProductCatalog/appsettings.json` matches the ProductServiceLibrary URL
3. Check CORS is enabled in ProductServiceLibrary (it is by default)

### Session data not persisting
ASP.NET Core session requires the session middleware to be configured in `Program.cs`. Verify `app.UseSession()` is called before `app.MapControllerRoute()`.

## Project Structure

```
ProductCatalogApp/
├── ProductServiceLibrary/          # .NET 10 Web API
│   ├── Controllers/
│   │   ├── ProductsController.cs
│   │   └── CategoriesController.cs
│   ├── Category.cs
│   ├── Product.cs
│   ├── ProductRepository.cs
│   ├── ProductService.cs
│   ├── IProductService.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── ProductServiceLibrary.csproj
│
├── ProductCatalog/                 # .NET 10 ASP.NET Core MVC
│   ├── Controllers/
│   │   └── HomeController.cs
│   ├── Models/
│   │   ├── CartItem.cs
│   │   └── Order.cs
│   ├── Services/
│   │   ├── ProductServiceClient.cs
│   │   └── OrderQueueService.cs
│   ├── Views/
│   │   ├── Home/
│   │   └── Shared/
│   ├── Program.cs
│   ├── SessionExtensions.cs
│   ├── appsettings.json
│   └── ProductCatalog.csproj
│
└── README.md
```

## Next Steps

- Configure Azure Service Bus for production
- Add authentication/authorization
- Implement database persistence for products
- Add logging and monitoring
- Deploy to Azure App Service or container platform
- Add automated tests

## License

This is a sample application for demonstration purposes.

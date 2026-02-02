# Product Catalog App - .NET 10 Migration

This application has been successfully migrated from .NET Framework 4.8.1 to .NET 10.

## Architecture

### ProductServiceLibrary (Web API)
- **Framework**: ASP.NET Core 10.0
- **Type**: RESTful Web API
- **Port**: 5000 (default)
- **Endpoints**:
  - `GET /api/products` - Get all products
  - `GET /api/products/{id}` - Get product by ID
  - `GET /api/products/category/{category}` - Get products by category
  - `GET /api/products/search?searchTerm={term}` - Search products
  - `GET /api/products/pricerange?minPrice={min}&maxPrice={max}` - Filter by price
  - `POST /api/products` - Create product
  - `PUT /api/products/{id}` - Update product
  - `DELETE /api/products/{id}` - Delete product
  - `GET /api/categories` - Get all categories

### ProductCatalog (MVC Web App)
- **Framework**: ASP.NET Core MVC 10.0
- **Type**: Server-side rendered web application
- **Port**: 5001 (default)
- **Features**:
  - Product browsing
  - Shopping cart with session management
  - Order checkout and submission

## Running the Application

### Prerequisites
- .NET 10 SDK

### Start the API
```bash
cd ProductServiceLibrary
dotnet run --urls "http://localhost:5000"
```

### Start the Web App
```bash
cd ProductCatalog
dotnet run --urls "http://localhost:5001"
```

### Configuration
Edit `ProductCatalog/appsettings.json` to configure:
- `ProductServiceUrl`: URL of the API service (default: http://localhost:5000)
- `OrderStoragePath`: Path for order storage (default: /tmp/ProductCatalogOrders)

## Migration Changes

### From WCF to REST API
- Replaced WCF service with ASP.NET Core Web API
- Migrated from SOAP to RESTful HTTP endpoints
- Removed `System.ServiceModel` dependencies

### From ASP.NET MVC to ASP.NET Core MVC
- Migrated views from Razor 3.x to Razor for .NET 10
- Replaced `System.Web.Mvc` with `Microsoft.AspNetCore.Mvc`
- Updated session management to use ASP.NET Core session

### Configuration Migration
- Replaced Web.config with appsettings.json
- Migrated app settings to JSON format

### Cross-Platform Compatibility
- Replaced MSMQ (Windows-only) with file-based order storage
- All features now work on Windows, Linux, and macOS

### Dependency Injection
- Implemented proper DI patterns throughout the application
- Repositories and services registered in DI container

## Known Limitations

1. **In-Memory Data**: Product data is stored in-memory and will reset on application restart
2. **File-Based Orders**: Orders are stored as JSON files in the file system
3. **No Authentication**: The application does not implement authentication/authorization
4. **CORS Policy**: Currently configured to allow all origins (should be restricted in production)

## Production Considerations

Before deploying to production:

1. **Database**: Replace in-memory storage with a real database (SQL Server, PostgreSQL, etc.)
2. **Message Queue**: Replace file-based order storage with a proper message queue (Azure Service Bus, RabbitMQ, etc.)
3. **Authentication**: Add authentication and authorization (Azure AD, Identity Server, etc.)
4. **CORS**: Restrict CORS to specific allowed origins
5. **HTTPS**: Enable HTTPS in production
6. **Logging**: Add structured logging (Serilog, Application Insights, etc.)
7. **Health Checks**: Implement health check endpoints
8. **Error Handling**: Add comprehensive error handling and monitoring

## Testing

The application has been tested with:
- Product listing ✅
- Add to cart ✅
- Update cart quantities ✅
- Remove from cart ✅
- Order submission ✅
- API endpoints ✅

## Original Framework

- **.NET Framework**: 4.8.1
- **ASP.NET MVC**: 5.2.9
- **WCF**: Windows Communication Foundation
- **MSMQ**: Microsoft Message Queue

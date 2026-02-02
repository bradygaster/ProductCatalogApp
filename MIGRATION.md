# ProductCatalogApp - .NET 10 Migration

This application has been migrated from .NET Framework 4.8.1 to .NET 10, with MSMQ replaced by Azure Service Bus.

## Architecture Changes

### ProductServiceLibrary
- **Before**: WCF Service (.NET Framework 4.8.1)
- **After**: ASP.NET Core Web API (.NET 10)
- **Changes**:
  - Converted from WCF ServiceContract to REST API controllers
  - Uses modern SDK-style project format
  - Added Swagger/OpenAPI support for API documentation

### ProductCatalog
- **Before**: ASP.NET MVC 5 (.NET Framework 4.8.1) with MSMQ
- **After**: ASP.NET Core MVC (.NET 10) with Azure Service Bus
- **Changes**:
  - Converted to ASP.NET Core MVC
  - Replaced WCF service reference with HTTP client
  - Migrated from MSMQ to Azure Service Bus for order processing
  - Replaced ASP.NET Session with ASP.NET Core Session with JSON serialization
  - Updated Razor views for ASP.NET Core Tag Helpers

## Azure Service Bus Configuration

### Prerequisites
1. Azure subscription
2. Azure Service Bus namespace created
3. Service Bus queue named `productcatalogorders` created

### Configuration Steps

1. **Create Azure Service Bus Namespace**:
   ```bash
   az servicebus namespace create \
     --name your-namespace \
     --resource-group your-resource-group \
     --location eastus \
     --sku Standard
   ```

2. **Create Queue**:
   ```bash
   az servicebus queue create \
     --namespace-name your-namespace \
     --name productcatalogorders \
     --resource-group your-resource-group
   ```

3. **Get Connection String**:
   ```bash
   az servicebus namespace authorization-rule keys list \
     --namespace-name your-namespace \
     --name RootManageSharedAccessKey \
     --resource-group your-resource-group \
     --query primaryConnectionString \
     --output tsv
   ```

4. **Update Configuration**:
   
   Edit `ProductCatalog/appsettings.json`:
   ```json
   {
     "ServiceBus": {
       "ConnectionString": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key",
       "QueueName": "productcatalogorders"
     }
   }
   ```

### Local Development

For local development without Azure Service Bus, you can:
1. Use Azure Service Bus Emulator (when available)
2. Use Azure Service Bus in development/test mode
3. Mock the OrderQueueService for testing

## Running the Application

### ProductServiceLibrary (API)

```bash
cd ProductServiceLibrary
dotnet run
```

The API will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger UI: https://localhost:5001/swagger

### ProductCatalog (Web App)

Update `ProductCatalog/appsettings.json` with your Azure Service Bus connection string, then:

```bash
cd ProductCatalog
dotnet run
```

The web application will be available at:
- HTTP: http://localhost:5000 (or the next available port)
- HTTPS: https://localhost:5001

## API Endpoints

### ProductServiceLibrary API

- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/category/{category}` - Get products by category
- `GET /api/products/search?searchTerm={term}` - Search products
- `GET /api/products/pricerange?minPrice={min}&maxPrice={max}` - Get products by price range
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product
- `GET /api/categories` - Get all categories

## Key Migration Notes

### MSMQ to Azure Service Bus

**Before (MSMQ)**:
```csharp
using System.Messaging;

var queue = new MessageQueue(@".\Private$\ProductCatalogOrders");
queue.Send(message);
```

**After (Azure Service Bus)**:
```csharp
using Azure.Messaging.ServiceBus;

var client = new ServiceBusClient(connectionString);
var sender = client.CreateSender(queueName);
await sender.SendMessageAsync(message);
```

### Session Management

**Before (ASP.NET)**:
```csharp
Session["Cart"] = cart;
var cart = Session["Cart"] as List<CartItem>;
```

**After (ASP.NET Core)**:
```csharp
HttpContext.Session.SetObjectAsJson("Cart", cart);
var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
```

### WCF to REST API

**Before (WCF Client)**:
```csharp
using (var client = new ProductServiceClient())
{
    var products = client.GetAllProducts();
}
```

**After (HTTP Client)**:
```csharp
var response = await _httpClient.GetAsync("/api/products");
var products = await response.Content.ReadFromJsonAsync<List<Product>>();
```

## Deployment Considerations

### Azure App Service
Both applications can be deployed to Azure App Service:
- ProductServiceLibrary as an API App
- ProductCatalog as a Web App

### Azure Kubernetes Service (AKS)
Both applications are containerizable for AKS deployment.

### Configuration
- Use Azure Key Vault for storing sensitive connection strings
- Use Azure App Configuration for centralized configuration management
- Enable Application Insights for monitoring and diagnostics

## Security Improvements

1. **Connection Strings**: Store in Azure Key Vault or User Secrets
2. **HTTPS**: Enforced by default in ASP.NET Core
3. **CORS**: Configure appropriately if API and Web App are on different domains
4. **Authentication**: Add authentication/authorization as needed (Azure AD, Identity, etc.)

## Testing

Build both projects:
```bash
dotnet build ProductServiceLibrary/ProductServiceLibrary.csproj
dotnet build ProductCatalog/ProductCatalog.csproj
```

## Troubleshooting

### Azure Service Bus Connection Issues
- Verify connection string is correct
- Check firewall rules allow access to Service Bus
- Ensure queue exists and has correct permissions

### API Connection Issues
- Verify ProductServiceUrl in appsettings.json points to running API
- Check CORS settings if API and Web App are on different domains
- Ensure API is running before starting Web App

## Next Steps

1. Add unit and integration tests
2. Implement proper error handling and logging
3. Add authentication and authorization
4. Set up CI/CD pipeline
5. Configure Application Insights
6. Add health checks for monitoring

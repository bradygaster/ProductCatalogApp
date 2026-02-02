# ProductCatalogApp - Quick Start Guide

## Prerequisites
- .NET 10 SDK
- Azure Service Bus namespace (for production use)

## Building the Solution

```bash
dotnet build
```

## Running Locally

### 1. Start the API (ProductServiceLibrary)

```bash
cd ProductServiceLibrary
dotnet run
```

The API will be available at:
- https://localhost:5001
- Swagger UI: https://localhost:5001/swagger

### 2. Configure Azure Service Bus

Edit `ProductCatalog/appsettings.json`:

```json
{
  "ProductServiceUrl": "https://localhost:5001",
  "ServiceBus": {
    "ConnectionString": "YOUR_AZURE_SERVICE_BUS_CONNECTION_STRING",
    "QueueName": "productcatalogorders"
  }
}
```

**For local testing without Azure**, you can temporarily modify the code to skip Azure Service Bus integration.

### 3. Start the Web App (ProductCatalog)

In a new terminal:

```bash
cd ProductCatalog
dotnet run
```

The web app will be available at the URL shown in the terminal output (typically https://localhost:5002 or next available port).

## Key Features

### Product Management
- Browse product catalog
- Add products to cart
- Update cart quantities
- Submit orders

### API Endpoints
View all available endpoints at: https://localhost:5001/swagger

### Order Processing
Orders are sent to Azure Service Bus queue for async processing.

## Development

### Project Structure

```
ProductCatalogApp/
├── ProductServiceLibrary/      # REST API (.NET 10)
│   ├── Controllers/            # API controllers
│   ├── Product.cs              # Product model
│   ├── Category.cs             # Category model
│   └── ProductRepository.cs    # In-memory data store
│
└── ProductCatalog/             # Web App (.NET 10)
    ├── Controllers/            # MVC controllers
    ├── Views/                  # Razor views
    ├── Models/                 # View models
    ├── Services/               # API client & queue service
    └── wwwroot/                # Static files
```

### Key Changes from .NET Framework
- **WCF → REST API**: Service layer now uses HTTP/REST instead of WCF
- **MSMQ → Azure Service Bus**: Async message processing modernized
- **ASP.NET → ASP.NET Core**: Modern web framework
- **Session Management**: JSON-based session serialization

## Testing

### Manual Testing
1. Start both applications
2. Navigate to the web app
3. Browse products
4. Add items to cart
5. Submit an order
6. Verify order appears in Azure Service Bus queue

### Using Azure Service Bus Explorer
Monitor your queue at: https://portal.azure.com → Service Bus → Queues → productcatalogorders

## Common Issues

### API Connection Failed
- Ensure ProductServiceLibrary is running
- Check `ProductServiceUrl` in appsettings.json
- Verify no firewall is blocking the connection

### Azure Service Bus Connection Failed
- Verify connection string is correct
- Ensure queue `productcatalogorders` exists
- Check network access to Azure

### Port Already in Use
Both apps will automatically select the next available port if the default is in use.

## Next Steps
- Review MIGRATION.md for detailed migration information
- Configure authentication/authorization as needed
- Set up CI/CD pipeline
- Deploy to Azure App Service or AKS

## Support
For issues and questions, please open an issue in the GitHub repository.

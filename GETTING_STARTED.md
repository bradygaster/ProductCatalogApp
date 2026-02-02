# Getting Started with ProductCatalogApp (.NET 10)

## Prerequisites

- .NET 10 SDK
- (Optional) Azure Service Bus namespace for order processing

## Quick Start

### 1. Clone and Build

```bash
git clone https://github.com/bradygaster/ProductCatalogApp.git
cd ProductCatalogApp
dotnet build
```

### 2. Configure Azure Service Bus (Optional)

To enable order submission functionality, edit `ProductCatalog/appsettings.json`:

```json
{
  "AzureServiceBus": {
    "ConnectionString": "Endpoint=sb://YOUR-NAMESPACE.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YOUR-KEY",
    "QueueName": "orders"
  }
}
```

**To get an Azure Service Bus connection string:**

1. Create an Azure Service Bus namespace in the [Azure Portal](https://portal.azure.com)
2. Navigate to your namespace → **Queues** → Create a queue named "orders"
3. Navigate to **Shared access policies** → **RootManageSharedAccessKey**
4. Copy the **Primary Connection String**

### 3. Run the Application

```bash
cd ProductCatalog
dotnet run
```

The application will start and display the URL (typically https://localhost:5001 or http://localhost:5000).

Open your browser and navigate to the URL.

## Using the Application

### Browse Products
- The home page displays all available products
- Products are organized by categories (Electronics, Clothing, Books, etc.)
- Each product shows:
  - Name, description, and category
  - Price
  - Stock availability
  - SKU

### Shopping Cart
1. Click "Add to Cart" on any product
2. Navigate to the Cart page using the navigation menu
3. In the cart, you can:
   - Update quantities
   - Remove items
   - Clear the entire cart
   - Submit an order

### Submit Orders
- Orders are sent to Azure Service Bus for processing
- If Azure Service Bus is not configured, you'll see an error message
- Successfully submitted orders display a confirmation with order ID

## Development Tips

### Running in Development Mode

The application is configured to run in Development mode by default, which includes:
- Detailed error pages
- Hot reload support
- Development-specific logging

### Environment Variables

You can override settings using environment variables:

```bash
# Windows (PowerShell)
$env:AzureServiceBus__ConnectionString="your-connection-string"
dotnet run

# Linux/macOS
export AzureServiceBus__ConnectionString="your-connection-string"
dotnet run
```

### User Secrets (Recommended for Local Development)

Instead of editing appsettings.json, use user secrets:

```bash
cd ProductCatalog
dotnet user-secrets init
dotnet user-secrets set "AzureServiceBus:ConnectionString" "your-connection-string"
dotnet run
```

## Project Structure

```
ProductCatalogApp/
├── ProductServiceLibrary/          # Product data and business logic
│   ├── Product.cs
│   ├── Category.cs
│   ├── ProductRepository.cs
│   ├── IProductService.cs
│   └── ProductService.cs
├── ProductCatalog/                 # ASP.NET Core MVC web application
│   ├── Controllers/
│   │   └── HomeController.cs
│   ├── Models/
│   │   ├── CartItem.cs
│   │   └── Order.cs
│   ├── Services/
│   │   └── OrderQueueService.cs
│   ├── Views/
│   │   ├── Home/
│   │   └── Shared/
│   ├── wwwroot/                    # Static files (CSS, JS)
│   ├── Program.cs
│   └── appsettings.json
├── ProductCatalogApp.sln
└── README.md
```

## Building for Production

### Publish the Application

```bash
dotnet publish -c Release -o ./publish
```

### Run the Published Application

```bash
cd publish
dotnet ProductCatalog.dll
```

### Configuration for Production

For production, set the following in your hosting environment:

1. **Connection Strings:** Use environment variables or Azure App Service configuration
2. **HTTPS:** Ensure HTTPS is properly configured
3. **Logging:** Configure appropriate logging levels
4. **Session State:** Consider using distributed cache (Redis) for session state in multi-instance deployments

## Troubleshooting

### Build Errors

**Problem:** "SDK not found"  
**Solution:** Install .NET 10 SDK from https://dotnet.microsoft.com/download

**Problem:** "Package restore failed"  
**Solution:** Run `dotnet restore` manually

### Runtime Errors

**Problem:** "Failed to send order to Service Bus"  
**Solution:** Check your Azure Service Bus connection string and ensure the queue exists

**Problem:** "Session not available"  
**Solution:** Ensure session middleware is configured (already set up in Program.cs)

### Azure Service Bus Issues

**Problem:** "Connection string is invalid"  
**Solution:** Verify the connection string format and ensure it includes SharedAccessKey

**Problem:** "Queue not found"  
**Solution:** Create the queue in Azure Service Bus namespace

## What's New in .NET 10

This application takes advantage of several .NET 10 features:

- **Minimal APIs:** Simplified Program.cs with top-level statements
- **Performance improvements:** Better throughput and lower memory usage
- **Enhanced async/await:** Improved async operations with Azure SDK
- **Improved dependency injection:** Built-in DI container improvements
- **Cross-platform support:** Runs on Windows, Linux, and macOS

## Support

For issues related to:
- **The application:** Open an issue on GitHub
- **.NET 10:** Visit [.NET Documentation](https://docs.microsoft.com/dotnet/)
- **Azure Service Bus:** Visit [Azure Service Bus Documentation](https://docs.microsoft.com/azure/service-bus-messaging/)

## License

See the repository license file for details.

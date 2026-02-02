# Product Catalog App - .NET 10 Migration

This repository contains a modernized Product Catalog application that has been upgraded from .NET Framework 4.8.1 to .NET 10, with MSMQ replaced by Azure Service Bus.

## What Changed

### Framework Upgrade
- **Before:** .NET Framework 4.8.1 (ASP.NET MVC 5)
- **After:** .NET 10 (ASP.NET Core MVC)

### Messaging System
- **Before:** Microsoft Message Queue (MSMQ)
- **After:** Azure Service Bus

### Architecture Changes
- **WCF Service:** Removed WCF-specific attributes and dependencies
- **Service Communication:** Direct library reference instead of WCF client/server
- **Configuration:** Migrated from Web.config to appsettings.json
- **Session Management:** Updated to use ASP.NET Core session

## Projects

### ProductServiceLibrary
A .NET 10 class library that provides product management functionality.
- Converted from WCF service to standard class library
- Removed ServiceContract, OperationContract, and DataContract attributes
- Uses standard .NET exceptions instead of FaultException

### ProductCatalog
An ASP.NET Core 10 MVC web application.
- Migrated from ASP.NET MVC 5 to ASP.NET Core MVC
- Updated session management to use ASP.NET Core sessions
- Migrated from Web.config to appsettings.json
- Uses dependency injection for ProductService

## Azure Service Bus Configuration

The application uses Azure Service Bus for order queue management. Configure the connection in `appsettings.json`:

```json
{
  "AzureServiceBus": {
    "ConnectionString": "YOUR_AZURE_SERVICE_BUS_CONNECTION_STRING",
    "QueueName": "orders"
  }
}
```

### Getting an Azure Service Bus Connection String

1. Create an Azure Service Bus namespace in the Azure Portal
2. Create a queue named "orders" (or use a different name and update the configuration)
3. Get the connection string from "Shared access policies" > "RootManageSharedAccessKey"

### Local Development

For local development without Azure Service Bus:
- The connection string can be left empty
- Order submission will fail with a clear error message
- All other features (browsing products, cart management) will work normally

## Building and Running

### Prerequisites
- .NET 10 SDK

### Build
```bash
dotnet build
```

### Run
```bash
cd ProductCatalog
dotnet run
```

The application will start on https://localhost:5001 (or the port specified in launchSettings.json).

## Key Features

- Browse product catalog with 22 sample products across 7 categories
- Add products to shopping cart
- Update quantities and remove items from cart
- Submit orders (sent to Azure Service Bus queue)
- Responsive Bootstrap UI

## Migration Notes

### Removed Components
- WCF service hosting (App.config from ProductServiceLibrary)
- Web.config (replaced with appsettings.json)
- ASP.NET MVC bundling and minification (using direct script references)
- packages.config (using PackageReference in .csproj)

### New Dependencies
- Azure.Messaging.ServiceBus (7.18.2)

### Breaking Changes
- Configuration keys have changed (Web.config → appsettings.json)
- MSMQ queue path configuration replaced with Azure Service Bus connection string
- Session access syntax changed for ASP.NET Core

## Future Enhancements

Potential improvements for further modernization:
- Add health checks
- Implement retry policies for Azure Service Bus
- Add structured logging (e.g., Serilog)
- Implement authentication and authorization
- Add API endpoints for external integrations
- Containerize with Docker
- Add unit and integration tests

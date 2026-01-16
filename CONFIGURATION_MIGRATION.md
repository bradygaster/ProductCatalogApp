# Configuration Migration Guide

## Overview

This project has been migrated to use `appsettings.json` configuration files alongside the traditional `web.config` and `app.config` files. This provides a more modern and flexible configuration approach.

## Configuration Files

### ProductCatalog (Web Application)

- **appsettings.json** - Base configuration for all environments
- **appsettings.Development.json** - Development-specific settings
- **appsettings.Staging.json** - Staging environment settings
- **appsettings.Production.json** - Production environment settings

### ProductServiceLibrary (WCF Service)

- **appsettings.json** - Base configuration for all environments
- **appsettings.Development.json** - Development-specific settings
- **appsettings.Staging.json** - Staging environment settings
- **appsettings.Production.json** - Production environment settings

## Environment Selection

The configuration system uses the `ASPNETCORE_ENVIRONMENT` environment variable to determine which environment-specific configuration file to load. If not set, it defaults to "Development".

To set the environment:

**Windows (IIS):**
```xml
<!-- In web.config -->
<configuration>
  <system.webServer>
    <aspNetCore>
      <environmentVariables>
        <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
      </environmentVariables>
    </aspNetCore>
  </system.webServer>
</configuration>
```

**Windows (Command Line):**
```cmd
set ASPNETCORE_ENVIRONMENT=Production
```

**PowerShell:**
```powershell
$env:ASPNETCORE_ENVIRONMENT="Production"
```

## Configuration Structure

### AppSettings Section

```json
{
  "AppSettings": {
    "OrderQueuePath": ".\\Private$\\ProductCatalogOrders"
  }
}
```

### WCF Client Configuration

```json
{
  "WcfClient": {
    "ProductService": {
      "Address": "http://localhost:8733/Design_Time_Addresses/ProductServiceLibrary/Service1/",
      "Binding": "basicHttpBinding"
    }
  }
}
```

### Logging Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

## Using Configuration in Code

The `AppConfiguration` class provides a centralized way to access configuration:

```csharp
using ProductCatalog;

// Get an app setting
string queuePath = AppConfiguration.GetAppSetting("OrderQueuePath");

// Access the configuration directly
var serviceAddress = AppConfiguration.Configuration["WcfClient:ProductService:Address"];
```

## Backward Compatibility

The code maintains backward compatibility with web.config/app.config. If a setting is not found in appsettings.json, it will fall back to the traditional configuration files:

```csharp
// Tries appsettings.json first, then web.config, then default value
_queuePath = AppConfiguration.GetAppSetting("OrderQueuePath") 
             ?? ConfigurationManager.AppSettings["OrderQueuePath"] 
             ?? @".\Private$\ProductCatalogOrders";
```

## NuGet Packages Added

The following Microsoft.Extensions.Configuration packages have been added to support JSON configuration:

- Microsoft.Extensions.Configuration (6.0.0)
- Microsoft.Extensions.Configuration.Abstractions (6.0.0)
- Microsoft.Extensions.Configuration.FileExtensions (6.0.0)
- Microsoft.Extensions.Configuration.Json (6.0.0)
- Microsoft.Extensions.FileProviders.Abstractions (6.0.0)
- Microsoft.Extensions.FileProviders.Physical (6.0.0)
- Microsoft.Extensions.FileSystemGlobbing (6.0.0)
- Microsoft.Extensions.Primitives (6.0.0)

## Migration Status

- ✅ Configuration files created for all environments
- ✅ NuGet packages added
- ✅ AppConfiguration helper class created
- ✅ OrderQueueService updated to use new configuration
- ✅ Backward compatibility maintained with web.config

## Next Steps

As you add more configurable settings to the application, add them to the appropriate appsettings.json files and update your code to use the `AppConfiguration` class to access them.

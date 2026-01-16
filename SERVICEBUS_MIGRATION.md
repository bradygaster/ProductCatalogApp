# Azure Service Bus Migration Guide

## Overview

This application has been migrated from MSMQ (Microsoft Message Queuing) to Azure Service Bus for cloud-native, scalable message processing. Azure Service Bus provides enterprise-level messaging with improved reliability, scalability, and cloud integration.

## What Changed

### 1. Messaging Infrastructure
- **Before:** MSMQ (`System.Messaging`) with local private queues
- **After:** Azure Service Bus with cloud-based queues

### 2. Serialization
- **Before:** XML serialization using `XmlMessageFormatter`
- **After:** JSON serialization using `System.Text.Json` (more secure and interoperable)

### 3. Configuration
- **Before:** `OrderQueuePath` in Web.config
- **After:** `ServiceBus:ConnectionString` and `ServiceBus:QueueName` in Web.config

### 4. NuGet Packages
- **Added:** Azure.Messaging.ServiceBus 7.20.1
- **Removed:** System.Messaging reference

## Prerequisites

### Azure Resources
1. **Azure Subscription** - Required to create Service Bus resources
2. **Azure Service Bus Namespace** - Created via Bicep template
3. **Service Bus Queue** - Named "product-orders"
4. **Azure Key Vault** (Recommended) - For secure connection string storage

## Deployment

### Option 1: Using Bicep (Recommended)

Deploy the Service Bus infrastructure using the provided Bicep template:

```bash
# Create resource group
az group create --name rg-productcatalog --location eastus

# Deploy Service Bus infrastructure
az deployment group create \
  --resource-group rg-productcatalog \
  --template-file infrastructure/service-bus.bicep \
  --parameters keyVaultName=kv-productcatalog-<unique-suffix>
```

### Option 2: Using Azure Portal

1. Navigate to Azure Portal (https://portal.azure.com)
2. Create a new **Service Bus Namespace**:
   - Name: `sb-productcatalog-<unique-suffix>`
   - Pricing tier: Standard (recommended for development)
   - Location: Your preferred region
3. Create a **Queue** in the namespace:
   - Name: `product-orders`
   - Max delivery count: 10
   - Message TTL: 14 days
   - Lock duration: 5 minutes
   - Enable dead lettering: Yes
4. Get the **connection string**:
   - Go to Shared access policies > RootManageSharedAccessKey
   - Copy the "Primary Connection String"

## Configuration

### Update Web.config

Replace the placeholder connection string in `Web.config` with your actual Service Bus connection string:

```xml
<appSettings>
  <add key="ServiceBus:ConnectionString" value="Endpoint=sb://YOUR-NAMESPACE.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YOUR-KEY" />
  <add key="ServiceBus:QueueName" value="product-orders" />
</appSettings>
```

### Using Azure Key Vault (Production)

For production environments, store the connection string in Azure Key Vault:

1. Create a Key Vault (if not exists)
2. Add a secret named `ServiceBusConnectionString`
3. Grant the web app's Managed Identity access to the Key Vault
4. Update Web.config to reference Key Vault:

```xml
<add key="ServiceBus:ConnectionString" value="@Microsoft.KeyVault(SecretUri=https://your-keyvault.vault.azure.net/secrets/ServiceBusConnectionString/)" />
```

## Code Changes

### OrderQueueService.cs

The `OrderQueueService` class has been completely rewritten to use Azure Service Bus:

**Key Changes:**
- Uses `ServiceBusClient`, `ServiceBusSender`, and `ServiceBusReceiver`
- Implements `IDisposable` for proper resource cleanup
- Replaces XML with JSON serialization
- Adds async/await pattern (with sync wrappers for backward compatibility)
- Built-in retry policies from Azure SDK

**Example Usage:**
```csharp
// Sending an order (synchronous - existing API)
using (var queueService = new OrderQueueService())
{
    queueService.SendOrder(order);
}

// Sending an order (asynchronous - new API)
using (var queueService = new OrderQueueService())
{
    await queueService.SendOrderAsync(order);
}

// Receiving an order
using (var queueService = new OrderQueueService())
{
    Order order = await queueService.ReceiveOrderAsync(TimeSpan.FromSeconds(30));
    if (order != null)
    {
        // Process order
    }
}
```

### HomeController.cs

Updated to properly dispose of `OrderQueueService`:

```csharp
// Now uses using statement for proper disposal
using (var queueService = new OrderQueueService())
{
    queueService.SendOrder(order);
}
```

## Testing

### Local Testing

For local development without Azure resources, you can use:

1. **Azure Service Bus Emulator** (if available)
2. **Azure Storage Queues** as a simpler alternative
3. **Mock connection string** for unit tests

### Integration Testing

1. Create a test Service Bus namespace in Azure
2. Update Web.config with test connection string
3. Run the application and submit an order
4. Verify message appears in Service Bus queue using Azure Portal

### Monitoring Messages

**Azure Portal:**
1. Navigate to your Service Bus namespace
2. Select "Queues" > "product-orders"
3. View "Active Messages" count and message details

**Service Bus Explorer:**
- Download and install Service Bus Explorer for advanced message inspection
- Connect using your connection string
- View, send, and receive test messages

## Migration Checklist

- [x] Azure.Messaging.ServiceBus package added
- [x] Service Bus namespace and queue created (via Bicep template)
- [x] Connection string stored in configuration
- [x] Producer code sends to Service Bus (OrderQueueService.SendOrder)
- [x] Consumer code receives from Service Bus (OrderQueueService.ReceiveOrder)
- [x] Error handling implemented with ServiceBusException
- [x] JSON serialization replaces XML
- [x] System.Messaging reference removed
- [x] IDisposable pattern implemented
- [ ] Integration tests validate message flow
- [ ] Connection string moved to Key Vault (recommended for production)

## Benefits of Azure Service Bus

1. **Cloud-Native:** No need for local MSMQ installation
2. **Scalability:** Auto-scales based on message volume
3. **Reliability:** Built-in redundancy and disaster recovery
4. **Security:** Supports Managed Identity, encryption at rest, and TLS
5. **Advanced Features:**
   - Message sessions for ordered processing
   - Dead-letter queues for failed messages
   - Scheduled message delivery
   - Message auto-forwarding
   - Topics and subscriptions for pub/sub patterns
6. **Monitoring:** Integrated with Azure Monitor and Application Insights
7. **Cross-Platform:** Works on Windows, Linux, and containers

## Troubleshooting

### Connection Issues

**Error:** "ServiceBus:ConnectionString is not configured"
- Verify Web.config has the correct appSettings key
- Check connection string format

**Error:** "Unauthorized access" or "401"
- Verify connection string is correct
- Check Service Bus namespace is active
- Ensure Shared Access Policy has Send/Listen permissions

### Message Processing Issues

**Messages not appearing:**
- Check queue name matches configuration
- Verify message was sent successfully (no exceptions)
- Check dead-letter queue for failed messages

**Slow processing:**
- Consider using async methods (`SendOrderAsync`, `ReceiveOrderAsync`)
- Increase max concurrent calls in processor configuration
- Enable prefetch for better throughput

### Performance Tuning

1. **Enable prefetch** for receivers to reduce round trips
2. **Use Standard or Premium tier** for better performance
3. **Implement batch sending** for multiple messages
4. **Use ServiceBusProcessor** for continuous processing instead of polling

## Next Steps

1. **Create a background processor** (Windows Service or Azure Function) to continuously process orders
2. **Implement dead-letter queue monitoring** for failed messages
3. **Add Application Insights** for distributed tracing
4. **Use Managed Identity** instead of connection strings
5. **Implement message versioning** for schema evolution
6. **Add circuit breaker pattern** for resilience

## Resources

- [Azure Service Bus Documentation](https://docs.microsoft.com/azure/service-bus-messaging/)
- [Azure.Messaging.ServiceBus SDK](https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/servicebus/Azure.Messaging.ServiceBus)
- [Service Bus Best Practices](https://docs.microsoft.com/azure/service-bus-messaging/service-bus-performance-improvements)
- [Migration Guide from MSMQ](https://docs.microsoft.com/azure/service-bus-messaging/migrate-from-msmq)

## Support

For issues or questions:
- Check Azure Service Bus status: https://status.azure.com/
- Azure Support: https://azure.microsoft.com/support/
- GitHub Issues: [Create an issue in this repository]

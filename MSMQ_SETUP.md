# Azure Service Bus Order Processing

## Overview
This application uses **Azure Service Bus** to handle order submissions asynchronously. When a customer clicks "Proceed to Checkout", the order is serialized to JSON and placed on an Azure Service Bus queue for backend processing.

> **Migration Note**: This application was previously using MSMQ (Microsoft Message Queuing) and has been modernized to use Azure Service Bus for better scalability, reliability, and cloud-native architecture.

## Setup Requirements

### 1. Azure Service Bus Resources

Deploy the required Azure resources using the Bicep template:

```bash
# Login to Azure
az login

# Create a resource group (if not exists)
az group create --name rg-productcatalog --location eastus

# Deploy the Service Bus infrastructure
az deployment group create \
  --resource-group rg-productcatalog \
  --template-file infrastructure/service-bus.bicep \
  --parameters serviceBusSku=Standard
```

Alternatively, you can create the resources manually in the Azure Portal:
1. Create a Service Bus namespace (Standard or Premium tier recommended)
2. Create a queue named `productcatalogorders`
3. Copy the connection string from the namespace settings

### 2. Add NuGet Packages

The following NuGet packages are required and already added to the project:
- `Azure.Messaging.ServiceBus` (v7.18.1)
- `System.Text.Json` (v8.0.4)
- Supporting packages (Azure.Core, System.Memory, etc.)

### 3. Configure Connection String

Update the `Web.config` file with your Service Bus connection string:

```xml
<appSettings>
  <add key="ServiceBus:ConnectionString" value="Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key" />
  <add key="ServiceBus:QueueName" value="productcatalogorders" />
</appSettings>
```

**Production Best Practice**: Store the connection string in Azure Key Vault:
1. Create an Azure Key Vault
2. Add the connection string as a secret
3. Use Azure App Configuration or Key Vault references in your app
4. Consider using Managed Identity for passwordless authentication

## How It Works

### Frontend (Web Application)

1. Customer adds items to cart
2. Customer clicks "Proceed to Checkout" on Cart page
3. `HomeController.SubmitOrder()` async action is called
4. Order is created from cart items with totals calculated
5. `OrderQueueService.SendOrderAsync()` serializes the order to JSON and sends it to Azure Service Bus
6. Cart is cleared and customer sees confirmation page

### Backend (Order Processor)

A separate backend application can process orders using `ServiceBusProcessor`:

```csharp
var client = new ServiceBusClient(connectionString);
var processor = client.CreateProcessor(queueName);

processor.ProcessMessageAsync += async args =>
{
    var orderJson = args.Message.Body.ToString();
    var order = JsonSerializer.Deserialize<Order>(orderJson);
    
    // Process order (payment, inventory, shipping, etc.)
    await ProcessOrderAsync(order);
    
    // Complete the message
    await args.CompleteMessageAsync(args.Message);
};

processor.ProcessErrorAsync += async args =>
{
    Console.WriteLine($"Error: {args.Exception.Message}");
    // Implement error handling/logging
};

await processor.StartProcessingAsync();
```

## Key Features

### Message Serialization
- **Format**: JSON (replaced XML/BinaryFormatter for security and compatibility)
- **Library**: System.Text.Json
- **Benefits**: Cross-platform, secure, human-readable

### Error Handling
- **Dead Letter Queue**: Automatically enabled for failed messages
- **Max Delivery Count**: 10 attempts before moving to DLQ
- **Message Lock Duration**: 5 minutes for processing
- **TTL**: 14 days message retention

### Scalability
- **Auto-scaling**: Azure Service Bus scales automatically
- **Concurrent Processing**: Configure max concurrent calls
- **Partitioning**: Optional for higher throughput
- **Sessions**: Disabled (can be enabled for ordered processing)

## Monitoring

### View Messages in Azure Portal
1. Navigate to your Service Bus namespace
2. Select "Queues" from the left menu
3. Click on `productcatalogorders`
4. View active messages, dead-letter messages, and metrics

### Enable Diagnostics
```bash
az monitor diagnostic-settings create \
  --resource /subscriptions/{sub-id}/resourceGroups/rg-productcatalog/providers/Microsoft.ServiceBus/namespaces/{namespace} \
  --name servicebus-diagnostics \
  --logs '[{"category": "OperationalLogs", "enabled": true}]'
```

### Metrics to Monitor
- **Active Messages**: Current queue depth
- **Incoming Messages**: Orders submitted per second
- **Outgoing Messages**: Orders processed per second
- **Dead Letter Messages**: Failed processing attempts
- **Server Errors**: Service Bus errors
- **User Errors**: Client/application errors

## Testing

### Local Development
1. Ensure Azure Service Bus namespace is created
2. Update `Web.config` with valid connection string
3. Build and run the ProductCatalog web application
4. Add items to cart
5. Click "Proceed to Checkout"
6. Verify order appears in Azure Portal (Service Bus Queue)

### Integration Testing
```csharp
// Example test
var testOrder = new Order { /* ... */ };
var queueService = new OrderQueueService(connectionString, queueName);
await queueService.SendOrderAsync(testOrder);

// Verify message in queue
var receivedOrder = await queueService.ReceiveOrderAsync(TimeSpan.FromSeconds(30));
Assert.IsNotNull(receivedOrder);
Assert.AreEqual(testOrder.OrderId, receivedOrder.OrderId);
```

## Migration from MSMQ

The following changes were made during migration:

### Code Changes
- ✅ Replaced `System.Messaging` with `Azure.Messaging.ServiceBus`
- ✅ Changed serialization from XML to JSON
- ✅ Updated to async/await pattern
- ✅ Added proper error handling and retry logic
- ✅ Implemented IDisposable pattern for resource cleanup

### Configuration Changes
- ✅ Added `ServiceBus:ConnectionString` to Web.config
- ✅ Added `ServiceBus:QueueName` to Web.config
- ✅ Kept legacy `OrderQueuePath` for reference (deprecated)

### Infrastructure Changes
- ✅ Created `infrastructure/service-bus.bicep` for Azure deployment
- ✅ Documented deployment and configuration procedures

### Benefits of Migration
- **Cloud-Native**: Fully managed service, no infrastructure to maintain
- **Scalability**: Auto-scales based on load
- **Reliability**: Built-in redundancy and disaster recovery
- **Security**: TLS 1.2+, Azure AD integration, Managed Identity support
- **Monitoring**: Rich metrics and diagnostics
- **Cost-Effective**: Pay only for what you use

## Production Considerations

- ✅ Store connection strings in Azure Key Vault
- ✅ Use Managed Identity instead of connection strings
- ✅ Implement comprehensive error handling
- ✅ Set up monitoring and alerts
- ✅ Configure dead letter queue processing
- ✅ Implement message versioning for schema changes
- ✅ Add logging for troubleshooting
- ✅ Test failover scenarios
- ✅ Document runbooks for common issues
- ✅ Set up backup and disaster recovery

## Files Modified

- **Services/OrderQueueService.cs** - Migrated from MSMQ to Service Bus
- **Controllers/HomeController.cs** - Updated to async/await pattern
- **Web.config** - Added Service Bus configuration
- **ProductCatalog.csproj** - Removed System.Messaging, added Service Bus SDK
- **packages.config** - Added Azure Service Bus packages

## Files Created

- **infrastructure/service-bus.bicep** - Azure infrastructure as code
- **infrastructure/README.md** - Infrastructure documentation

## Troubleshooting

### Common Issues

**Error: "ServiceBus:ConnectionString not configured"**
- Solution: Ensure Web.config has valid Service Bus connection string

**Error: "Unauthorized access"**
- Solution: Verify connection string has correct permissions (Send/Listen)

**Error: "Queue not found"**
- Solution: Ensure queue name matches in both Azure and Web.config

**Messages going to Dead Letter Queue**
- Solution: Check `ProcessErrorAsync` logs for processing errors
- Verify message format is correct JSON
- Check max delivery count settings

### Support Resources
- [Azure Service Bus Documentation](https://docs.microsoft.com/azure/service-bus-messaging/)
- [Best Practices](https://docs.microsoft.com/azure/service-bus-messaging/service-bus-performance-improvements)
- [Troubleshooting Guide](https://docs.microsoft.com/azure/service-bus-messaging/service-bus-messaging-exceptions)


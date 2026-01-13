# Azure Service Bus Migration Guide

## Overview
This application has been migrated from Microsoft Message Queuing (MSMQ) to Azure Service Bus for cloud-native, scalable message queuing. When a customer clicks "Proceed to Checkout", the order is serialized to JSON and sent to an Azure Service Bus queue for asynchronous backend processing.

## Prerequisites

### 1. Azure Service Bus Namespace

You need an Azure Service Bus namespace with a queue. You can create these resources using:

#### Option A: Azure Portal
1. Navigate to the Azure Portal
2. Create a new "Service Bus Namespace"
3. Within the namespace, create a queue named `product-catalog-orders`
4. Copy the connection string from "Shared access policies" > "RootManageSharedAccessKey"

#### Option B: Azure CLI
```bash
# Set variables
RESOURCE_GROUP="product-catalog-rg"
LOCATION="eastus"
NAMESPACE_NAME="product-catalog-sb"
QUEUE_NAME="product-catalog-orders"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create Service Bus namespace
az servicebus namespace create \
  --resource-group $RESOURCE_GROUP \
  --name $NAMESPACE_NAME \
  --location $LOCATION \
  --sku Standard

# Create queue
az servicebus queue create \
  --resource-group $RESOURCE_GROUP \
  --namespace-name $NAMESPACE_NAME \
  --name $QUEUE_NAME

# Get connection string
az servicebus namespace authorization-rule keys list \
  --resource-group $RESOURCE_GROUP \
  --namespace-name $NAMESPACE_NAME \
  --name RootManageSharedAccessKey \
  --query primaryConnectionString \
  --output tsv
```

#### Option C: Bicep Infrastructure as Code
Use the provided Bicep template at `infrastructure/service-bus.bicep`:

```bash
# Deploy the infrastructure
az deployment group create \
  --resource-group product-catalog-rg \
  --template-file infrastructure/service-bus.bicep \
  --parameters serviceBusNamespaceName=product-catalog-sb
```

### 2. Configure Connection Strings

#### ProductCatalog Web Application
Update `Web.config` with your Service Bus connection string:

```xml
<appSettings>
  <add key="ServiceBusConnectionString" value="Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key" />
  <add key="ServiceBusQueueName" value="product-catalog-orders" />
</appSettings>
```

**Best Practice**: Store the connection string in Azure Key Vault and use Managed Identity in production:

```csharp
// Example using Azure.Identity and Azure.Extensions.AspNetCore.Configuration.Secrets
var keyVaultEndpoint = new Uri("https://your-keyvault.vault.azure.net/");
builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());
```

#### OrderProcessor Console Application
Update `App.config` with your Service Bus connection string:

```xml
<appSettings>
  <add key="ServiceBusConnectionString" value="Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key" />
  <add key="ServiceBusQueueName" value="product-catalog-orders" />
</appSettings>
```

### 3. NuGet Package Restore

Both projects now reference the Azure Service Bus SDK:

```
Azure.Messaging.ServiceBus v7.17.0
Azure.Core v1.35.0
Newtonsoft.Json v13.0.3
```

**In Visual Studio:**
1. Open the solution
2. Right-click the solution and select "Restore NuGet Packages"
3. Build the solution

**Using .NET CLI (if targeting Windows with .NET Framework SDK):**
```bash
nuget restore ProductCatalogApp.sln
msbuild ProductCatalogApp.sln /p:Configuration=Release
```

## Architecture Changes

### Message Serialization
- **Before**: XML Serialization using `XmlMessageFormatter`
- **After**: JSON Serialization using `Newtonsoft.Json`

### Producer (ProductCatalog Web App)

**Before (MSMQ):**
```csharp
using (MessageQueue queue = new MessageQueue(@".\Private$\ProductCatalogOrders"))
{
    queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(Order) });
    queue.Send(new Message(order));
}
```

**After (Azure Service Bus):**
```csharp
await using var client = new ServiceBusClient(connectionString);
var sender = client.CreateSender(queueName);
string messageBody = JsonConvert.SerializeObject(order);
await sender.SendMessageAsync(new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody)));
```

### Consumer (OrderProcessor App)

**Before (MSMQ):**
```csharp
using (MessageQueue queue = new MessageQueue(@".\Private$\ProductCatalogOrders"))
{
    queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(Order) });
    Message message = queue.Receive(timeout);
    Order order = (Order)message.Body;
}
```

**After (Azure Service Bus):**
```csharp
var processor = client.CreateProcessor(queueName);
processor.ProcessMessageAsync += async args =>
{
    string messageBody = Encoding.UTF8.GetString(args.Message.Body);
    Order order = JsonConvert.DeserializeObject<Order>(messageBody);
    // Process order...
    await args.CompleteMessageAsync(args.Message);
};
await processor.StartProcessingAsync();
```

## Key Features

### Error Handling
- **Dead Letter Queue**: Messages that fail to deserialize are automatically sent to the dead letter queue
- **Retry Logic**: Failed messages are abandoned and automatically retried based on queue configuration
- **Max Delivery Count**: Configured to 10 attempts before dead lettering

### Scalability
- **Concurrent Processing**: Processor can be configured for multiple concurrent message handlers
- **Auto Lock Renewal**: Messages are automatically kept locked during long-running operations
- **Partitioning**: Can be enabled for higher throughput

### Monitoring
- Service Bus provides built-in metrics in Azure Portal
- Monitor queue depth, message rates, and errors
- Set up alerts for queue length thresholds

## Testing

### 1. Build and Run ProductCatalog Web Application
```bash
# In Visual Studio, press F5 or
# Using IIS Express command line:
iisexpress /path:C:\path\to\ProductCatalog /port:44320
```

### 2. Run OrderProcessor Console Application
```bash
# In Visual Studio, set OrderProcessor as startup project and press F5 or
# From command line:
cd OrderProcessor\bin\Debug
OrderProcessor.exe
```

### 3. Submit Test Orders
1. Navigate to https://localhost:44320
2. Add products to cart
3. Click "Proceed to Checkout"
4. Verify order appears in OrderProcessor console

### 4. Monitor in Azure Portal
1. Navigate to your Service Bus namespace
2. Click on your queue
3. View active messages, dead letter queue, and metrics

## Migration Checklist

- [x] Azure Service Bus SDK added to both projects
- [x] Service Bus namespace and queue created
- [x] Connection strings configured
- [x] Producer code migrated to Service Bus
- [x] Consumer code migrated to Service Bus
- [x] XML serialization replaced with JSON
- [x] Error handling implemented
- [x] MSMQ references removed

## Production Considerations

### Security
- ✅ Use Managed Identity instead of connection strings
- ✅ Store secrets in Azure Key Vault
- ✅ Use least-privilege SAS policies (separate sender/receiver policies)
- ✅ Enable Virtual Network service endpoints

### Reliability
- ✅ Configure appropriate message TTL (default: 14 days)
- ✅ Monitor dead letter queue
- ✅ Implement retry policies with exponential backoff
- ✅ Set up alerting for queue depth

### Performance
- ✅ Use batching for high-throughput scenarios
- ✅ Enable partitioning for very high throughput
- ✅ Configure prefetch for better performance
- ✅ Consider Premium tier for guaranteed IOPS

### Cost Optimization
- ✅ Use Basic tier for development/testing
- ✅ Use Standard tier for production (includes topics)
- ✅ Use Premium tier only if you need dedicated resources
- ✅ Monitor and optimize message sizes

## Troubleshooting

### Connection Issues
- Verify connection string format
- Check firewall rules and network security groups
- Ensure Service Bus namespace is active

### Message Processing Issues
- Check dead letter queue for failed messages
- Verify JSON serialization/deserialization
- Review processor error handler logs

### Build Issues
- Ensure all NuGet packages are restored
- Check .NET Framework version (requires 4.8.1)
- Verify assembly binding redirects in config files

## Additional Resources

- [Azure Service Bus Documentation](https://docs.microsoft.com/azure/service-bus-messaging/)
- [Azure.Messaging.ServiceBus SDK](https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/servicebus/Azure.Messaging.ServiceBus)
- [Best Practices for Service Bus](https://docs.microsoft.com/azure/service-bus-messaging/service-bus-performance-improvements)
- [Service Bus Pricing](https://azure.microsoft.com/pricing/details/service-bus/)

## Legacy MSMQ Documentation

For the legacy MSMQ implementation, see [MSMQ_SETUP.md](MSMQ_SETUP.md) (deprecated).

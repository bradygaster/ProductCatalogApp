# Order Processor Backend Application

This console application processes orders from the Azure Service Bus queue populated by the ProductCatalog web application.

## Features

- Continuously monitors the Azure Service Bus queue for new orders
- Processes orders asynchronously with simulated steps:
  - Payment validation
  - Inventory updates
  - Shipping label creation
  - Confirmation email sending
- Displays detailed order information
- Implements error handling with dead letter queue
- Automatic message retry on failures
- Graceful shutdown with 'Q' key

## Setup

### 1. Prerequisites

- .NET Framework 4.8.1
- Azure Service Bus namespace with a queue
- Connection string for the Service Bus namespace

### 2. Configure Azure Service Bus

Update the `App.config` file with your Service Bus settings:

```xml
<appSettings>
  <add key="ServiceBusConnectionString" value="Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key" />
  <add key="ServiceBusQueueName" value="product-catalog-orders" />
</appSettings>
```

**Security Note**: For production, store the connection string in Azure Key Vault and use Managed Identity.

### 3. Build the Application

**In Visual Studio:**
1. Open the solution
2. Right-click the OrderProcessor project and select "Restore NuGet Packages"
3. Build the project (Ctrl+Shift+B)

**Using Command Line:**
```bash
nuget restore OrderProcessor.csproj
msbuild OrderProcessor.csproj /p:Configuration=Release
```

### 4. NuGet Dependencies

The project requires the following packages (automatically restored):
- Azure.Messaging.ServiceBus v7.17.0
- Azure.Core v1.35.0
- Newtonsoft.Json v13.0.3

## Running the Processor

### Development
1. Build the project
2. Run the executable: `OrderProcessor.exe`
3. The application will:
   - Connect to Azure Service Bus
   - Display a waiting message
   - Process orders as they arrive
   - Show detailed order information for each processed order
4. Press 'Q' to gracefully shutdown

### Production - Windows Service
For production, consider running as a Windows Service:
1. Use a service wrapper like TopShelf or NSSM
2. Configure automatic restart on failure
3. Set up proper logging and monitoring

## Output Example

```
===========================================
  Product Catalog Order Processor
  (Azure Service Bus Edition)
===========================================

Queue Name: product-catalog-orders
Service Bus client initialized successfully.
Service Bus connected. Waiting for orders...
Press 'Q' to quit

=======================================
  NEW ORDER RECEIVED: 12345678-1234-1234-1234-123456789012
=======================================

Order Date:    2024-01-15 14:30:22
Session ID:    xyz123

Order Items:
---------------------------------------
  * Laptop (SKU: LAP-001)
    Quantity: 1 x $999.99 = $999.99
  * Mouse (SKU: MOU-002)
    Quantity: 2 x $29.99 = $59.98
---------------------------------------
Subtotal:      $1,059.97
Tax:           $84.80
Shipping:      $0.00
TOTAL:         $1,144.77

Processing order...
  [14:30:25] Validating payment... DONE
  [14:30:26] Updating inventory... DONE
  [14:30:27] Creating shipping label... DONE
  [14:30:28] Sending confirmation email... DONE
[SUCCESS] Order 12345678-1234-1234-1234-123456789012 processed successfully!
```

## Architecture

### Message Processing Flow

1. **Receive**: ServiceBusProcessor receives messages asynchronously
2. **Deserialize**: JSON message body is deserialized to Order object
3. **Process**: Order processing logic executes (simulated steps)
4. **Complete**: Message is marked as complete and removed from queue
5. **Error Handling**:
   - Deserialization errors → Dead letter queue
   - Processing errors → Abandon (retry automatically)

### Error Handling

```csharp
processor.ProcessMessageAsync += async args =>
{
    try
    {
        // Deserialize and process order
        await args.CompleteMessageAsync(args.Message);
    }
    catch (JsonException ex)
    {
        // Dead letter the message if it can't be deserialized
        await args.DeadLetterMessageAsync(args.Message, "DeserializationError", ex.Message);
    }
    catch (Exception ex)
    {
        // Abandon the message so it can be retried
        await args.AbandonMessageAsync(args.Message);
    }
};
```

### Configuration Options

The ServiceBusProcessor is configured with:
- **MaxConcurrentCalls**: 1 (process one message at a time)
- **AutoCompleteMessages**: false (manual completion for better control)
- **MaxAutoLockRenewalDuration**: 5 minutes (auto-extend message lock)

## Monitoring

### Azure Portal
1. Navigate to your Service Bus namespace
2. Select your queue
3. Monitor:
   - Active message count
   - Dead letter message count
   - Incoming/outgoing message rate
   - Processing errors

### Dead Letter Queue
Check dead letter queue for failed messages:
```bash
az servicebus queue show \
  --resource-group product-catalog-rg \
  --namespace-name your-namespace \
  --name product-catalog-orders/$DeadLetterQueue
```

## Production Enhancements

For production use, consider:

### 1. Persistent Storage
```csharp
// Save order to database before/after processing
await _orderRepository.SaveAsync(order);
```

### 2. Comprehensive Logging
```csharp
// Use structured logging
_logger.LogInformation("Processing order {OrderId} for customer {SessionId}", 
    order.OrderId, order.CustomerSessionId);
```

### 3. Real Integrations
- Connect to real payment gateway (Stripe, PayPal, etc.)
- Update actual inventory database
- Send real emails via SendGrid/Azure Communication Services
- Integrate with shipping APIs (FedEx, UPS, etc.)

### 4. Performance Optimization
```csharp
// Increase concurrent processing
var options = new ServiceBusProcessorOptions
{
    MaxConcurrentCalls = 10,
    PrefetchCount = 20
};
```

### 5. Observability
- Application Insights integration
- Custom metrics for processing time
- Alerting for high queue depth or processing errors

### 6. Health Checks
```csharp
// Implement health check endpoint for monitoring
public async Task<bool> CheckHealthAsync()
{
    try
    {
        // Test Service Bus connectivity
        await _client.CreateSender(_queueName).SendMessageAsync(healthCheckMessage);
        return true;
    }
    catch { return false; }
}
```

## Troubleshooting

### Connection Issues
- **Error**: "Endpoint not found"
  - **Solution**: Verify connection string format and namespace name

- **Error**: "Unauthorized access"
  - **Solution**: Check SAS key in connection string or Managed Identity permissions

### Message Processing Issues
- **Messages in dead letter queue**
  - Check for serialization/deserialization errors
  - Review error messages in dead letter queue properties

- **Messages not being processed**
  - Verify processor is started: `await processor.StartProcessingAsync()`
  - Check for exceptions in error handler

### Build Issues
- **Missing assembly references**
  - Restore NuGet packages: `nuget restore`
  - Check package versions in packages.config

- **Binding redirect errors**
  - Verify assembly binding redirects in App.config
  - Update redirect versions if needed

## Migration from MSMQ

This application was migrated from MSMQ. Key changes:

| Aspect | MSMQ | Azure Service Bus |
|--------|------|-------------------|
| Queue Location | Local machine | Azure cloud |
| Message Format | XML | JSON |
| Connection | Queue path | Connection string |
| Processing | Synchronous pull | Async push (processor) |
| Error Handling | Manual | Dead letter queue |
| Scalability | Single machine | Cloud scale |

For the legacy MSMQ documentation, see the repository's MSMQ_SETUP.md file.

## Additional Resources

- [Azure Service Bus Documentation](https://docs.microsoft.com/azure/service-bus-messaging/)
- [Azure.Messaging.ServiceBus SDK](https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/servicebus/Azure.Messaging.ServiceBus)
- [Best Practices for Service Bus](https://docs.microsoft.com/azure/service-bus-messaging/service-bus-performance-improvements)
- [Complete Migration Guide](../AZURE_SERVICEBUS_SETUP.md)


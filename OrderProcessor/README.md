# Order Processor Backend Application

This console application processes orders from the MSMQ queue populated by the ProductCatalog web application.

## Features

- Continuously monitors the MSMQ queue for new orders
- Processes orders with simulated steps:
  - Payment validation
  - Inventory updates
  - Shipping label creation
  - Confirmation email sending
- Displays detailed order information
- Graceful shutdown with 'Q' key

## Setup

### 1. Build the Application

This is a .NET Framework 4.8.1 console application. You'll need to:

1. Create a new Console Application (.NET Framework) project in Visual Studio
2. Target .NET Framework 4.8.1
3. Add reference to **System.Messaging** assembly
4. Copy the `Program.cs` and `App.config` files to your project

### 2. Configure Queue Path

The `App.config` contains the queue path setting:

```xml
<add key="OrderQueuePath" value=".\Private$\ProductCatalogOrders" />
```

This must match the queue path configured in the ProductCatalog web application's Web.config.

### 3. Prerequisites

- MSMQ must be installed and enabled on Windows
- The queue will be automatically created if it doesn't exist
- Ensure you have permissions to access the queue

## Running the Processor

1. Build the project
2. Run the executable (OrderProcessor.exe)
3. The application will:
   - Connect to the MSMQ queue
   - Display a waiting message
   - Process orders as they arrive
   - Show detailed order information for each processed order

4. Press 'Q' to gracefully shutdown

## Output Example

```
===========================================
  Product Catalog Order Processor
===========================================

Queue Path: .\Private$\ProductCatalogOrders
Queue exists.
Queue is ready. Waiting for orders...
Press 'Q' to quit

??????????????????????????????????????????????????????????????????
  NEW ORDER RECEIVED: 12345678-1234-1234-1234-123456789012
??????????????????????????????????????????????????????????????????

Order Date:    2024-01-15 14:30:22
Session ID:    xyz123

Order Items:
?????????????????????????????????????????????????????????????????
  • Laptop (SKU: LAP-001)
    Quantity: 1 x $999.99 = $999.99
  • Mouse (SKU: MOU-002)
    Quantity: 2 x $29.99 = $59.98
?????????????????????????????????????????????????????????????????
Subtotal:      $1,059.97
Tax:           $84.80
Shipping:      $0.00
TOTAL:         $1,144.77

Processing order...
  [14:30:25] Validating payment... DONE
  [14:30:26] Updating inventory... DONE
  [14:30:27] Creating shipping label... DONE
  [14:30:28] Sending confirmation email... DONE
? Order 12345678-1234-1234-1234-123456789012 processed successfully!
```

## Production Enhancements

For production use, consider:

1. **Persistent Storage**: Save orders to a database
2. **Error Handling**: Implement retry logic and dead letter queue handling
3. **Logging**: Add comprehensive logging (e.g., Serilog, NLog)
4. **Monitoring**: Track processing times and queue depth
5. **Windows Service**: Convert to a Windows Service for always-on processing
6. **Transactions**: Use transactional message processing
7. **Real Integration**: 
   - Connect to real payment gateway
   - Update actual inventory database
   - Send real emails via SMTP
   - Integrate with shipping APIs

## Converting to Windows Service

To run this as a Windows Service:

1. Create a new Windows Service project
2. Move the processing logic to the service's `OnStart()` and `OnStop()` methods
3. Install using `sc.exe` or InstallUtil.exe
4. Configure to start automatically

## Troubleshooting

- **Queue not found**: Ensure MSMQ is installed and the queue path is correct
- **Access denied**: Run with appropriate permissions or configure queue permissions
- **No messages received**: Verify the web application is successfully sending messages
- **Serialization errors**: Ensure Order classes are marked `[Serializable]` and match between projects

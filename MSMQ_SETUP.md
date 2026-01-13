# MSMQ Order Processing Setup

## Overview
This application now uses Microsoft Message Queuing (MSMQ) to handle order submissions asynchronously. When a customer clicks "Proceed to Checkout", the order is serialized and placed on an MSMQ queue for backend processing.

## Manual Setup Required

### 1. Add Experimental.System.Messaging Package to ProductCatalog Project

Since this is now an ASP.NET Core project targeting .NET 10.0, we use the Experimental.System.Messaging NuGet package:

**Via command line:**
```bash
cd ProductCatalog
dotnet add package Experimental.System.Messaging
```

**OR via project file:**
The package is already included in ProductCatalog.csproj:
```xml
<PackageReference Include="Experimental.System.Messaging" Version="1.1.0" />
```

**Note:** Experimental.System.Messaging is a community-maintained port of System.Messaging for .NET Core/.NET. It provides MSMQ functionality but is Windows-only.

### 2. Enable MSMQ on Windows

MSMQ must be installed and enabled on your Windows machine:

**Windows 10/11:**
1. Open "Control Panel" > "Programs" > "Turn Windows features on or off"
2. Expand "Microsoft Message Queue (MSMQ) Server"
3. Check "Microsoft Message Queue (MSMQ) Server Core"
4. Click OK and let Windows install the feature
5. Restart if prompted

**Windows Server:**
1. Open Server Manager
2. Add Roles and Features
3. Select "Message Queuing" under Features
4. Complete the installation

### 3. Queue Configuration

The application uses a private queue named: `.\Private$\ProductCatalogOrders`

This queue is automatically created when the first order is submitted. You can configure the queue path in appsettings.json:

```json
{
  "AppSettings": {
    "OrderQueuePath": ".\\Private$\\ProductCatalogOrders"
  }
}
```

### 4. Viewing the Queue

To view messages in the queue:
1. Open Computer Management (compmgmt.msc)
2. Expand "Services and Applications" > "Message Queuing"
3. Expand "Private Queues"
4. Find "productcatalogorders"
5. Right-click > "Message Queue Messages" to view queued orders

## How It Works

### Frontend (Web Application)

1. Customer adds items to cart
2. Customer clicks "Proceed to Checkout" on Cart page
3. `HomeController.SubmitOrder()` action is called
4. Order is created from cart items with totals calculated
5. `OrderQueueService.SendOrder()` serializes the order to XML and sends it to MSMQ
6. Cart is cleared and customer sees confirmation page

### Backend (Order Processor)

A separate backend application (console app or Windows Service) can process orders:

1. Continuously polls the MSMQ queue using `OrderQueueService.ReceiveOrder()`
2. Processes each order (payment, inventory update, shipping, etc.)
3. Sends confirmation emails
4. Updates order status in database

See the `OrderProcessor` console application in this solution for a working example.

## Files Added

- **Models/Order.cs** - Order and OrderItem models (serializable for MSMQ)
- **Services/OrderQueueService.cs** - MSMQ service for sending/receiving orders
- **Views/Home/OrderConfirmation.cshtml** - Order confirmation page
- **HomeController.cs** - Added SubmitOrder() and OrderConfirmation() actions
- **OrderProcessor/** - Backend console app to process queued orders

## Testing

1. Ensure MSMQ is installed and running
2. Build and run the ProductCatalog web application
3. Add items to cart
4. Click "Proceed to Checkout"
5. Verify order appears in MSMQ (Computer Management)
6. Run the OrderProcessor console app to process the order

## Production Considerations

- Implement proper error handling and dead letter queue monitoring
- Add order persistence to database before/after queuing
- Implement retry logic for failed messages
- Set up transactional queues for critical operations
- Monitor queue depth and processing times
- Consider using Windows Service for backend processor
- Implement proper logging and alerting

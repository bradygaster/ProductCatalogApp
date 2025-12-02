using System;
using System.Configuration;
using System.Messaging;
using System.Threading;

namespace OrderProcessor
{
    // Copy these classes from ProductCatalog.Models or reference the assembly
    [Serializable]
    public class Order
    {
        public string OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public System.Collections.Generic.List<OrderItem> Items { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Shipping { get; set; }
        public decimal Total { get; set; }
        public string CustomerSessionId { get; set; }
    }

    [Serializable]
    public class OrderItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string SKU { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }

    class Program
    {
        private static string _queuePath;
        private static bool _running = true;

        static void Main(string[] args)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("  Product Catalog Order Processor");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            // Get queue path from config or use default
            _queuePath = ConfigurationManager.AppSettings["OrderQueuePath"] ?? @".\Private$\ProductCatalogOrders";
            
            Console.WriteLine($"Queue Path: {_queuePath}");
            Console.WriteLine();

            // Ensure queue exists
            if (!EnsureQueueExists())
            {
                Console.WriteLine("Failed to access or create queue. Press any key to exit.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Queue is ready. Waiting for orders...");
            Console.WriteLine("Press 'Q' to quit");
            Console.WriteLine();

            // Start processing in background thread
            Thread processingThread = new Thread(ProcessOrders);
            processingThread.Start();

            // Wait for quit command
            while (_running)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Q)
                    {
                        Console.WriteLine("\nShutting down...");
                        _running = false;
                    }
                }
                Thread.Sleep(100);
            }

            // Wait for processing thread to finish
            processingThread.Join(5000);
            Console.WriteLine("Processor stopped.");
        }

        private static bool EnsureQueueExists()
        {
            try
            {
                if (!MessageQueue.Exists(_queuePath))
                {
                    Console.WriteLine($"Queue does not exist. Creating queue: {_queuePath}");
                    MessageQueue.Create(_queuePath);
                    Console.WriteLine("Queue created successfully.");
                }
                else
                {
                    Console.WriteLine("Queue exists.");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with queue: {ex.Message}");
                return false;
            }
        }

        private static void ProcessOrders()
        {
            using (MessageQueue queue = new MessageQueue(_queuePath))
            {
                queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(Order) });

                while (_running)
                {
                    try
                    {
                        // Try to receive message with 2 second timeout
                        Message message = queue.Receive(TimeSpan.FromSeconds(2));
                        Order order = (Order)message.Body;

                        ProcessOrder(order);
                    }
                    catch (MessageQueueException ex) when (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                    {
                        // No message available - this is normal, just continue
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Failed to receive/process message: {ex.Message}");
                    }
                }
            }
        }

        private static void ProcessOrder(Order order)
        {
            Console.WriteLine("??????????????????????????????????????????????????????????????????");
            Console.WriteLine($"  NEW ORDER RECEIVED: {order.OrderId}");
            Console.WriteLine("??????????????????????????????????????????????????????????????????");
            Console.WriteLine();
            
            Console.WriteLine($"Order Date:    {order.OrderDate:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Session ID:    {order.CustomerSessionId}");
            Console.WriteLine();
            
            Console.WriteLine("Order Items:");
            Console.WriteLine("?????????????????????????????????????????????????????????????????");
            
            foreach (var item in order.Items)
            {
                Console.WriteLine($"  • {item.ProductName} (SKU: {item.SKU})");
                Console.WriteLine($"    Quantity: {item.Quantity} x ${item.Price:N2} = ${item.Subtotal:N2}");
            }
            
            Console.WriteLine("?????????????????????????????????????????????????????????????????");
            Console.WriteLine($"Subtotal:      ${order.Subtotal:N2}");
            Console.WriteLine($"Tax:           ${order.Tax:N2}");
            Console.WriteLine($"Shipping:      ${order.Shipping:N2}");
            Console.WriteLine($"TOTAL:         ${order.Total:N2}");
            Console.WriteLine();

            // Simulate order processing steps
            Console.WriteLine("Processing order...");
            
            SimulateProcessingStep("Validating payment", 1000);
            SimulateProcessingStep("Updating inventory", 800);
            SimulateProcessingStep("Creating shipping label", 1200);
            SimulateProcessingStep("Sending confirmation email", 500);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"? Order {order.OrderId} processed successfully!");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();
        }

        private static void SimulateProcessingStep(string stepName, int delayMs)
        {
            Console.Write($"  [{DateTime.Now:HH:mm:ss}] {stepName}...");
            Thread.Sleep(delayMs);
            Console.WriteLine(" DONE");
        }
    }
}

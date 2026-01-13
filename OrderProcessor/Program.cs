using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        private static string _connectionString;
        private static string _queueName;
        private static bool _running = true;
        private static ServiceBusClient _client;
        private static ServiceBusProcessor _processor;

        static async Task Main(string[] args)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("  Product Catalog Order Processor");
            Console.WriteLine("  (Azure Service Bus Edition)");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            // Get Service Bus configuration
            _connectionString = ConfigurationManager.AppSettings["ServiceBusConnectionString"];
            _queueName = ConfigurationManager.AppSettings["ServiceBusQueueName"] ?? "product-catalog-orders";

            if (string.IsNullOrEmpty(_connectionString))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: ServiceBusConnectionString is not configured in App.config");
                Console.ResetColor();
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"Queue Name: {_queueName}");
            Console.WriteLine();

            // Initialize Service Bus client
            if (!await InitializeServiceBusAsync())
            {
                Console.WriteLine("Failed to initialize Service Bus. Press any key to exit.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Service Bus connected. Waiting for orders...");
            Console.WriteLine("Press 'Q' to quit");
            Console.WriteLine();

            // Start processing
            await _processor.StartProcessingAsync();

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
                await Task.Delay(100);
            }

            // Stop processing and cleanup
            await _processor.StopProcessingAsync();
            await _processor.DisposeAsync();
            await _client.DisposeAsync();
            
            Console.WriteLine("Processor stopped.");
        }

        private static async Task<bool> InitializeServiceBusAsync()
        {
            try
            {
                _client = new ServiceBusClient(_connectionString);
                
                // Create processor with options
                var options = new ServiceBusProcessorOptions
                {
                    MaxConcurrentCalls = 1,
                    AutoCompleteMessages = false,
                    MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5)
                };

                _processor = _client.CreateProcessor(_queueName, options);
                
                // Configure event handlers
                _processor.ProcessMessageAsync += MessageHandler;
                _processor.ProcessErrorAsync += ErrorHandler;
                
                Console.WriteLine("Service Bus client initialized successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error initializing Service Bus: {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }

        private static async Task MessageHandler(ProcessMessageEventArgs args)
        {
            try
            {
                // Get message body
                string messageBody = Encoding.UTF8.GetString(args.Message.Body);
                
                // Deserialize order
                Order order = JsonConvert.DeserializeObject<Order>(messageBody);
                
                // Process the order
                ProcessOrder(order);
                
                // Complete the message
                await args.CompleteMessageAsync(args.Message);
            }
            catch (JsonException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] Failed to deserialize message: {ex.Message}");
                Console.ResetColor();
                
                // Dead letter the message if it can't be deserialized
                await args.DeadLetterMessageAsync(args.Message, "DeserializationError", ex.Message);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] Failed to process message: {ex.Message}");
                Console.ResetColor();
                
                // Abandon the message so it can be retried
                await args.AbandonMessageAsync(args.Message);
            }
        }

        private static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] Message handler error: {args.Exception.Message}");
            Console.WriteLine($"  Entity Path: {args.EntityPath}");
            Console.WriteLine($"  Error Source: {args.ErrorSource}");
            Console.ResetColor();
            return Task.CompletedTask;
        }

        private static void ProcessOrder(Order order)
        {
            Console.WriteLine("=======================================");
            Console.WriteLine($"  NEW ORDER RECEIVED: {order.OrderId}");
            Console.WriteLine("=======================================");
            Console.WriteLine();
            
            Console.WriteLine($"Order Date:    {order.OrderDate:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Session ID:    {order.CustomerSessionId}");
            Console.WriteLine();
            
            Console.WriteLine("Order Items:");
            Console.WriteLine("---------------------------------------");
            
            foreach (var item in order.Items)
            {
                Console.WriteLine($"  * {item.ProductName} (SKU: {item.SKU})");
                Console.WriteLine($"    Quantity: {item.Quantity} x ${item.Price:N2} = ${item.Subtotal:N2}");
            }
            
            Console.WriteLine("---------------------------------------");
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
            Console.WriteLine($"[SUCCESS] Order {order.OrderId} processed successfully!");
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

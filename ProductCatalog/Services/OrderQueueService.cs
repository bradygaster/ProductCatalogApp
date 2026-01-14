using ProductCatalog.Models;
using System;
using Microsoft.Extensions.Configuration;

namespace ProductCatalog.Services
{
    public class OrderQueueService
    {
        private readonly string _queuePath;

        public OrderQueueService()
        {
            _queuePath = @".\Private$\ProductCatalogOrders";
            EnsureQueueExists();
        }

        public OrderQueueService(IConfiguration configuration)
        {
            _queuePath = configuration["AppSettings:OrderQueuePath"] ?? @".\Private$\ProductCatalogOrders";
            EnsureQueueExists();
        }

        public OrderQueueService(string queuePath)
        {
            _queuePath = queuePath;
            EnsureQueueExists();
        }

        private void EnsureQueueExists()
        {
            try
            {
                // MSMQ support in ASP.NET Core requires platform-specific implementation
                // For now, we'll log that the queue is being used
                System.Diagnostics.Debug.WriteLine($"Queue path configured: {_queuePath}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create or access message queue at {_queuePath}", ex);
            }
        }

        public void SendOrder(Order order)
        {
            try
            {
                // MSMQ functionality - This requires System.Messaging which is Windows-specific
                // In a real ASP.NET Core app, you would use a cross-platform queue like RabbitMQ or Azure Service Bus
                // For now, we'll serialize the order to a file as a temporary measure
                var ordersDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Orders");
                Directory.CreateDirectory(ordersDirectory);
                
                var orderFile = Path.Combine(ordersDirectory, $"{order.OrderId}.json");
                var orderJson = Newtonsoft.Json.JsonConvert.SerializeObject(order, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(orderFile, orderJson);
                
                System.Diagnostics.Debug.WriteLine($"Order {order.OrderId} saved to file: {orderFile}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send order {order.OrderId} to queue", ex);
            }
        }

        public Order ReceiveOrder(TimeSpan timeout)
        {
            try
            {
                // MSMQ functionality - simplified for ASP.NET Core
                var ordersDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Orders");
                if (!Directory.Exists(ordersDirectory))
                    return null;
                
                var orderFiles = Directory.GetFiles(ordersDirectory, "*.json");
                if (orderFiles.Length == 0)
                    return null;
                
                var orderFile = orderFiles[0];
                var orderJson = File.ReadAllText(orderFile);
                var order = Newtonsoft.Json.JsonConvert.DeserializeObject<Order>(orderJson);
                File.Delete(orderFile);
                
                return order;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to receive order from queue", ex);
            }
        }

        public int GetQueueMessageCount()
        {
            try
            {
                var ordersDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Orders");
                if (!Directory.Exists(ordersDirectory))
                    return 0;
                
                return Directory.GetFiles(ordersDirectory, "*.json").Length;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}

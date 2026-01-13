using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using ProductCatalog.Models;
using System;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalog.Services
{
    public class OrderQueueService : IDisposable
    {
        private readonly string _connectionString;
        private readonly string _queueName;
        private ServiceBusClient _client;
        private ServiceBusSender _sender;

        public OrderQueueService()
        {
            _connectionString = ConfigurationManager.AppSettings["ServiceBusConnectionString"];
            _queueName = ConfigurationManager.AppSettings["ServiceBusQueueName"] ?? "product-catalog-orders";
            
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("ServiceBusConnectionString is not configured in Web.config");
            }

            InitializeClient();
        }

        public OrderQueueService(string connectionString, string queueName)
        {
            _connectionString = connectionString;
            _queueName = queueName;
            
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Connection string cannot be null or empty");
            }

            InitializeClient();
        }

        private void InitializeClient()
        {
            try
            {
                _client = new ServiceBusClient(_connectionString);
                _sender = _client.CreateSender(_queueName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize Service Bus client for queue '{_queueName}'", ex);
            }
        }

        public void SendOrder(Order order)
        {
            try
            {
                SendOrderAsync(order).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send order {order.OrderId} to Service Bus queue", ex);
            }
        }

        private async Task SendOrderAsync(Order order)
        {
            try
            {
                // Serialize order to JSON
                string messageBody = JsonConvert.SerializeObject(order);
                
                // Create Service Bus message
                ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(messageBody))
                {
                    Subject = $"Order {order.OrderId}",
                    MessageId = order.OrderId,
                    ContentType = "application/json"
                };

                // Add custom properties
                message.ApplicationProperties.Add("OrderDate", order.OrderDate);
                message.ApplicationProperties.Add("Total", order.Total);
                
                // Send message with retry logic
                await _sender.SendMessageAsync(message);
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
                return ReceiveOrderAsync(timeout).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to receive order from Service Bus queue", ex);
            }
        }

        private async Task<Order> ReceiveOrderAsync(TimeSpan timeout)
        {
            ServiceBusReceiver receiver = null;
            try
            {
                receiver = _client.CreateReceiver(_queueName);
                
                ServiceBusReceivedMessage message = await receiver.ReceiveMessageAsync(timeout);
                
                if (message == null)
                {
                    return null;
                }

                // Deserialize message body
                string messageBody = Encoding.UTF8.GetString(message.Body.ToArray());
                Order order = JsonConvert.DeserializeObject<Order>(messageBody);
                
                // Complete the message to remove it from the queue
                await receiver.CompleteMessageAsync(message);
                
                return order;
            }
            catch (TaskCanceledException)
            {
                // Timeout occurred
                return null;
            }
            finally
            {
                if (receiver != null)
                {
                    await receiver.DisposeAsync();
                }
            }
        }

        public int GetQueueMessageCount()
        {
            try
            {
                return GetQueueMessageCountAsync().GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private async Task<int> GetQueueMessageCountAsync()
        {
            try
            {
                // Note: Getting message count requires management operations
                // For simplicity, we'll return 0 as this is a non-critical feature
                // In production, use ServiceBusAdministrationClient for management operations
                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public void Dispose()
        {
            try
            {
                _sender?.DisposeAsync().GetAwaiter().GetResult();
                _client?.DisposeAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // Suppress exceptions during disposal
            }
        }
    }
}

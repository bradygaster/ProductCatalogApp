using ProductCatalog.Models;
using System;
using System.Configuration;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace ProductCatalog.Services
{
    public class OrderQueueService : IDisposable
    {
        private readonly string _connectionString;
        private readonly string _queueName;
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;

        public OrderQueueService()
        {
            _connectionString = ConfigurationManager.AppSettings["ServiceBus:ConnectionString"];
            _queueName = ConfigurationManager.AppSettings["ServiceBus:QueueName"] ?? "product-orders";
            
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("ServiceBus:ConnectionString is not configured in Web.config");
            }

            _client = new ServiceBusClient(_connectionString);
            _sender = _client.CreateSender(_queueName);
        }

        public OrderQueueService(string connectionString, string queueName)
        {
            _connectionString = connectionString;
            _queueName = queueName;
            _client = new ServiceBusClient(_connectionString);
            _sender = _client.CreateSender(_queueName);
        }

        public void SendOrder(Order order)
        {
            SendOrderAsync(order).GetAwaiter().GetResult();
        }

        public async Task SendOrderAsync(Order order)
        {
            try
            {
                // Serialize order to JSON
                string messageBody = JsonSerializer.Serialize(order);
                
                // Create Service Bus message
                var serviceBusMessage = new ServiceBusMessage(messageBody)
                {
                    MessageId = order.OrderId,
                    Subject = $"Order {order.OrderId}",
                    ContentType = "application/json"
                };

                // Add custom properties
                serviceBusMessage.ApplicationProperties.Add("OrderId", order.OrderId);
                serviceBusMessage.ApplicationProperties.Add("OrderDate", order.OrderDate);
                serviceBusMessage.ApplicationProperties.Add("Total", order.Total);

                // Send message to Service Bus with retry policy (built-in)
                await _sender.SendMessageAsync(serviceBusMessage);
            }
            catch (ServiceBusException ex)
            {
                throw new InvalidOperationException($"Failed to send order {order.OrderId} to Service Bus queue", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send order {order.OrderId} to queue", ex);
            }
        }

        public Order ReceiveOrder(TimeSpan timeout)
        {
            return ReceiveOrderAsync(timeout).GetAwaiter().GetResult();
        }

        public async Task<Order> ReceiveOrderAsync(TimeSpan timeout)
        {
            ServiceBusReceiver receiver = null;
            try
            {
                receiver = _client.CreateReceiver(_queueName);
                
                // Receive message with timeout
                ServiceBusReceivedMessage message = await receiver.ReceiveMessageAsync(timeout);
                
                if (message == null)
                {
                    return null;
                }

                // Deserialize message body
                string messageBody = message.Body.ToString();
                Order order = JsonSerializer.Deserialize<Order>(messageBody);

                // Complete the message (remove from queue)
                await receiver.CompleteMessageAsync(message);

                return order;
            }
            catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.ServiceTimeout)
            {
                return null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to receive order from Service Bus queue", ex);
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
            return GetQueueMessageCountAsync().GetAwaiter().GetResult();
        }

        public async Task<int> GetQueueMessageCountAsync()
        {
            try
            {
                // Note: Getting message count requires management operations
                // For production, use Azure.Messaging.ServiceBus.Administration
                // For now, return 0 as this is not critical functionality
                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public void Dispose()
        {
            _sender?.DisposeAsync().GetAwaiter().GetResult();
            _client?.DisposeAsync().GetAwaiter().GetResult();
        }
    }
}

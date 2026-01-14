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
        private ServiceBusClient _client;
        private ServiceBusSender _sender;

        public OrderQueueService()
        {
            _connectionString = ConfigurationManager.AppSettings["ServiceBus:ConnectionString"] 
                ?? throw new InvalidOperationException("ServiceBus:ConnectionString not configured");
            _queueName = ConfigurationManager.AppSettings["ServiceBus:QueueName"] 
                ?? "productcatalogorders";
            
            InitializeClient();
        }

        public OrderQueueService(string connectionString, string queueName)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
            
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
                throw new InvalidOperationException($"Failed to initialize Service Bus client for queue {_queueName}", ex);
            }
        }

        public async Task SendOrderAsync(Order order)
        {
            try
            {
                var messageBody = JsonSerializer.Serialize(order);
                var message = new ServiceBusMessage(messageBody)
                {
                    Subject = $"Order {order.OrderId}",
                    MessageId = order.OrderId,
                    ContentType = "application/json"
                };

                await _sender.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send order {order.OrderId} to queue", ex);
            }
        }

        // Synchronous wrapper for backward compatibility
        public void SendOrder(Order order)
        {
            SendOrderAsync(order).GetAwaiter().GetResult();
        }

        public async Task<Order> ReceiveOrderAsync(TimeSpan timeout)
        {
            ServiceBusReceiver receiver = null;
            try
            {
                receiver = _client.CreateReceiver(_queueName);
                var message = await receiver.ReceiveMessageAsync(timeout);
                
                if (message == null)
                {
                    return null;
                }

                var order = JsonSerializer.Deserialize<Order>(message.Body.ToString());
                await receiver.CompleteMessageAsync(message);
                
                return order;
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is TimeoutException)
            {
                return null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to receive order from queue", ex);
            }
            finally
            {
                if (receiver != null)
                {
                    await receiver.DisposeAsync();
                }
            }
        }

        // Synchronous wrapper for backward compatibility
        public Order ReceiveOrder(TimeSpan timeout)
        {
            return ReceiveOrderAsync(timeout).GetAwaiter().GetResult();
        }

        public async Task<int> GetQueueMessageCountAsync()
        {
            try
            {
                var receiver = _client.CreateReceiver(_queueName);
                var count = 0;
                
                // Note: Service Bus doesn't provide direct message count API
                // This is a simplified implementation that peeks messages
                await foreach (var message in receiver.PeekMessagesAsync(100))
                {
                    count++;
                }
                
                await receiver.DisposeAsync();
                return count;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        // Synchronous wrapper for backward compatibility
        public int GetQueueMessageCount()
        {
            return GetQueueMessageCountAsync().GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            _sender?.DisposeAsync().GetAwaiter().GetResult();
            _client?.DisposeAsync().GetAwaiter().GetResult();
        }
    }
}

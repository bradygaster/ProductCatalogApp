using Azure.Messaging.ServiceBus;
using ProductCatalog.Models;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProductCatalog.Services
{
    public class OrderQueueService : IAsyncDisposable
    {
        private readonly string _connectionString;
        private readonly string _queueName;
        private ServiceBusClient _client;
        private ServiceBusSender _sender;
        private ServiceBusReceiver _receiver;

        public OrderQueueService(string connectionString, string queueName = "orders")
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
            }

            _connectionString = connectionString;
            _queueName = queueName;
            
            InitializeClient();
        }

        private void InitializeClient()
        {
            try
            {
                _client = new ServiceBusClient(_connectionString);
                _sender = _client.CreateSender(_queueName);
                _receiver = _client.CreateReceiver(_queueName);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize Azure Service Bus client for queue '{_queueName}'", ex);
            }
        }

        public async Task SendOrderAsync(Order order)
        {
            try
            {
                if (order == null)
                {
                    throw new ArgumentNullException(nameof(order));
                }

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
                throw new InvalidOperationException($"Failed to send order {order?.OrderId} to Service Bus queue", ex);
            }
        }

        public async Task<Order> ReceiveOrderAsync(TimeSpan timeout)
        {
            try
            {
                var message = await _receiver.ReceiveMessageAsync(timeout);
                
                if (message == null)
                {
                    return null;
                }

                var order = JsonSerializer.Deserialize<Order>(message.Body.ToString());
                
                // Complete the message to remove it from the queue
                await _receiver.CompleteMessageAsync(message);
                
                return order;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to receive order from Service Bus queue", ex);
            }
        }

        public async Task<long> GetQueueMessageCountAsync()
        {
            try
            {
                var queueProperties = await _client.CreateReceiver(_queueName).PeekMessageAsync();
                return queueProperties != null ? 1 : 0; // Simplified for demo
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_sender != null)
            {
                await _sender.DisposeAsync();
            }
            
            if (_receiver != null)
            {
                await _receiver.DisposeAsync();
            }
            
            if (_client != null)
            {
                await _client.DisposeAsync();
            }
        }
    }
}

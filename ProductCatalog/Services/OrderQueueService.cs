using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using ProductCatalog.Models;
using System;
using System.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProductCatalog.Services
{
    public class OrderQueueService : IDisposable
    {
        private readonly string _connectionString;
        private readonly string _queueName;
        private ServiceBusClient _client;
        private ServiceBusSender _sender;
        private readonly SemaphoreSlim _disposeLock = new SemaphoreSlim(1, 1);
        private bool _disposed = false;

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
                // Use Task.Run to avoid deadlock in ASP.NET synchronization context
                Task.Run(async () => await SendOrderAsync(order).ConfigureAwait(false)).GetAwaiter().GetResult();
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
                await _sender.SendMessageAsync(message).ConfigureAwait(false);
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
                // Use Task.Run to avoid deadlock in ASP.NET synchronization context
                return Task.Run(async () => await ReceiveOrderAsync(timeout).ConfigureAwait(false)).GetAwaiter().GetResult();
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
                
                ServiceBusReceivedMessage message = await receiver.ReceiveMessageAsync(timeout).ConfigureAwait(false);
                
                if (message == null)
                {
                    return null;
                }

                // Deserialize message body
                string messageBody = Encoding.UTF8.GetString(message.Body.ToArray());
                Order order = JsonConvert.DeserializeObject<Order>(messageBody);
                
                // Complete the message to remove it from the queue
                await receiver.CompleteMessageAsync(message).ConfigureAwait(false);
                
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
                    await receiver.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        public int GetQueueMessageCount()
        {
            // Note: Getting message count requires ServiceBusAdministrationClient with management operations
            // This would require additional permissions and a different connection string
            // For simplicity and to avoid potential deadlocks, we return -1 to indicate unavailable
            // In production, implement this using ServiceBusAdministrationClient if needed
            return -1;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Dispose managed resources synchronously using a dedicated thread
                // This avoids blocking the ASP.NET synchronization context
                var disposeTask = Task.Run(async () =>
                {
                    await _disposeLock.WaitAsync().ConfigureAwait(false);
                    try
                    {
                        if (_sender != null)
                        {
                            await _sender.DisposeAsync().ConfigureAwait(false);
                            _sender = null;
                        }

                        if (_client != null)
                        {
                            await _client.DisposeAsync().ConfigureAwait(false);
                            _client = null;
                        }
                    }
                    finally
                    {
                        _disposeLock.Release();
                    }
                });

                // Wait for disposal to complete with a timeout
                if (!disposeTask.Wait(TimeSpan.FromSeconds(30)))
                {
                    // Log warning if needed - disposal timed out
                }

                _disposeLock?.Dispose();
            }

            _disposed = true;
        }
    }
}

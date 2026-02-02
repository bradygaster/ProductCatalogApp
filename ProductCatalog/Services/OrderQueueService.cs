using Azure.Messaging.ServiceBus;
using ProductCatalog.Models;
using System.Text.Json;

namespace ProductCatalog.Services;

public interface IOrderQueueService
{
    Task SendOrderAsync(Order order);
    Task<Order?> ReceiveOrderAsync(TimeSpan timeout);
}

public class OrderQueueService : IOrderQueueService
{
    private readonly string _connectionString;
    private readonly string _queueName;
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;

    public OrderQueueService(IConfiguration configuration)
    {
        _connectionString = configuration["ServiceBusConnectionString"] ?? throw new InvalidOperationException("ServiceBusConnectionString not configured");
        _queueName = configuration["OrderQueueName"] ?? "productcatalogorders";
        
        _client = new ServiceBusClient(_connectionString);
        _sender = _client.CreateSender(_queueName);
    }

    public async Task SendOrderAsync(Order order)
    {
        try
        {
            var json = JsonSerializer.Serialize(order);
            var message = new ServiceBusMessage(json)
            {
                Subject = $"Order {order.OrderId}",
                ContentType = "application/json"
            };

            await _sender.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to send order {order.OrderId} to queue", ex);
        }
    }

    public async Task<Order?> ReceiveOrderAsync(TimeSpan timeout)
    {
        ServiceBusReceiver? receiver = null;
        try
        {
            receiver = _client.CreateReceiver(_queueName);
            var message = await receiver.ReceiveMessageAsync(timeout);
            
            if (message == null)
            {
                return null;
            }

            var json = message.Body.ToString();
            var order = JsonSerializer.Deserialize<Order>(json);
            
            await receiver.CompleteMessageAsync(message);
            
            return order;
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
}

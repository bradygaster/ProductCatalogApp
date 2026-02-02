using ProductCatalog.Models;
using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace ProductCatalog.Services;

public class OrderQueueService
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly string _queueName;

    public OrderQueueService(ServiceBusClient serviceBusClient, IConfiguration configuration)
    {
        _serviceBusClient = serviceBusClient;
        _queueName = configuration["ServiceBus:QueueName"] ?? "productcatalogorders";
    }

    public async Task SendOrderAsync(Order order)
    {
        try
        {
            await using var sender = _serviceBusClient.CreateSender(_queueName);
            
            var messageBody = JsonSerializer.Serialize(order);
            var message = new ServiceBusMessage(messageBody)
            {
                Subject = $"Order {order.OrderId}",
                MessageId = order.OrderId,
                ContentType = "application/json"
            };

            await sender.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to send order {order.OrderId} to queue", ex);
        }
    }

    public async Task<Order?> ReceiveOrderAsync(TimeSpan timeout)
    {
        try
        {
            await using var receiver = _serviceBusClient.CreateReceiver(_queueName);
            
            var message = await receiver.ReceiveMessageAsync(timeout);
            if (message == null)
            {
                return null;
            }

            var order = JsonSerializer.Deserialize<Order>(message.Body.ToString());
            
            // Complete the message to remove it from the queue
            await receiver.CompleteMessageAsync(message);
            
            return order;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to receive order from queue", ex);
        }
    }

    public async Task<int> GetQueueMessageCountAsync()
    {
        try
        {
            // Note: Getting queue message count from Service Bus requires management library
            // For now, we'll return 0. In production, use Azure.Messaging.ServiceBus.Administration
            return 0;
        }
        catch (Exception)
        {
            return 0;
        }
    }
}

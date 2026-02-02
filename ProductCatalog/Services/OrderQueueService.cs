using ProductCatalog.Models;
using System.Text.Json;

namespace ProductCatalog.Services;

public class OrderQueueService
{
    private readonly string _orderStoragePath;
    private static readonly object _lock = new();

    public OrderQueueService(IConfiguration configuration)
    {
        var basePath = configuration["OrderStoragePath"] ?? Path.Combine(Path.GetTempPath(), "ProductCatalogOrders");
        _orderStoragePath = basePath;
        EnsureStorageExists();
    }

    private void EnsureStorageExists()
    {
        try
        {
            if (!Directory.Exists(_orderStoragePath))
            {
                Directory.CreateDirectory(_orderStoragePath);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create order storage directory at {_orderStoragePath}", ex);
        }
    }

    public void SendOrder(Order order)
    {
        try
        {
            lock (_lock)
            {
                var fileName = $"order_{order.OrderId}_{DateTime.Now:yyyyMMddHHmmss}.json";
                var filePath = Path.Combine(_orderStoragePath, fileName);
                var json = JsonSerializer.Serialize(order, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save order {order.OrderId}", ex);
        }
    }

    public Order? ReceiveOrder(TimeSpan timeout)
    {
        try
        {
            lock (_lock)
            {
                var files = Directory.GetFiles(_orderStoragePath, "order_*.json")
                    .OrderBy(f => File.GetCreationTime(f))
                    .ToArray();

                if (files.Length == 0)
                {
                    return null;
                }

                var filePath = files[0];
                var json = File.ReadAllText(filePath);
                var order = JsonSerializer.Deserialize<Order>(json);
                File.Delete(filePath);
                return order;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to receive order from storage", ex);
        }
    }

    public int GetQueueMessageCount()
    {
        try
        {
            lock (_lock)
            {
                return Directory.GetFiles(_orderStoragePath, "order_*.json").Length;
            }
        }
        catch (Exception)
        {
            return 0;
        }
    }
}

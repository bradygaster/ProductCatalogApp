using ProductCatalog.Models;
using Newtonsoft.Json;

namespace ProductCatalog.Services
{
    public class OrderQueueService
    {
        private readonly string _queuePath;

        public OrderQueueService(IConfiguration configuration)
        {
            var queuePath = configuration["OrderQueuePath"];
            _queuePath = string.IsNullOrEmpty(queuePath) ? Path.Combine(Path.GetTempPath(), "ProductCatalogOrders") : queuePath;
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
                if (!Directory.Exists(_queuePath))
                {
                    Directory.CreateDirectory(_queuePath);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create or access order queue directory at {_queuePath}", ex);
            }
        }

        public void SendOrder(Order order)
        {
            try
            {
                var fileName = Path.Combine(_queuePath, $"Order_{order.OrderId}_{DateTime.Now:yyyyMMddHHmmss}.json");
                var json = JsonConvert.SerializeObject(order, Formatting.Indented);
                File.WriteAllText(fileName, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send order {order.OrderId} to queue", ex);
            }
        }

        public Order? ReceiveOrder()
        {
            try
            {
                var files = Directory.GetFiles(_queuePath, "Order_*.json").OrderBy(f => f).ToArray();
                if (files.Length == 0)
                {
                    return null;
                }

                var fileName = files[0];
                var json = File.ReadAllText(fileName);
                var order = JsonConvert.DeserializeObject<Order>(json);
                File.Delete(fileName);
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
                return Directory.GetFiles(_queuePath, "Order_*.json").Length;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}

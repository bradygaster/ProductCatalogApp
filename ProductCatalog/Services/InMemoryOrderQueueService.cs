using ProductCatalog.Models;
using System;
using System.Collections.Concurrent;

namespace ProductCatalog.Services
{
    public interface IOrderQueueService
    {
        void SendOrder(Order order);
        Order ReceiveOrder(TimeSpan timeout);
        int GetQueueMessageCount();
    }

    public class InMemoryOrderQueueService : IOrderQueueService
    {
        private readonly ConcurrentQueue<Order> _queue = new ConcurrentQueue<Order>();

        public void SendOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));
            
            _queue.Enqueue(order);
        }

        public Order ReceiveOrder(TimeSpan timeout)
        {
            if (_queue.TryDequeue(out var order))
                return order;
            
            return null;
        }

        public int GetQueueMessageCount()
        {
            return _queue.Count;
        }
    }
}

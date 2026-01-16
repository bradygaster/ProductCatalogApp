using ProductCatalog.Models;
using Experimental.System.Messaging;

namespace ProductCatalog.Services
{
    public class OrderQueueService
    {
        private readonly string _queuePath;

        public OrderQueueService()
        {
            _queuePath = @".\Private$\ProductCatalogOrders";
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
                if (!MessageQueue.Exists(_queuePath))
                {
                    MessageQueue.Create(_queuePath);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create or access message queue at {_queuePath}", ex);
            }
        }

        public void SendOrder(Order order)
        {
            try
            {
                using (MessageQueue queue = new MessageQueue(_queuePath))
                {
                    queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(Order) });
                    
                    Message message = new Message(order)
                    {
                        Label = $"Order {order.OrderId}",
                        Recoverable = true
                    };

                    queue.Send(message);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send order {order.OrderId} to queue", ex);
            }
        }

        public Order? ReceiveOrder(TimeSpan timeout)
        {
            try
            {
                using (MessageQueue queue = new MessageQueue(_queuePath))
                {
                    queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(Order) });
                    
                    Message message = queue.Receive(timeout);
                    return (Order)message.Body;
                }
            }
            catch (MessageQueueException ex) when (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
            {
                return null;
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
                using (MessageQueue queue = new MessageQueue(_queuePath))
                {
                    return queue.GetAllMessages().Length;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}

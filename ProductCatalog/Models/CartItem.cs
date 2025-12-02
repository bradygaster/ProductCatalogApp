using System;
using ProductCatalog.ProductServiceReference;

namespace ProductCatalog.Models
{
    public class CartItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }

        public decimal Subtotal
        {
            get
            {
                return Product != null ? Product.Price * Quantity : 0;
            }
        }
    }
}

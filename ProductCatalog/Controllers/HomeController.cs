using ProductCatalog.Models;
using ProductCatalog.ProductServiceReference;
using ProductCatalog.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ProductCatalog.Controllers
{
    public class HomeController : Controller
    {
        // Helper methods for session management in ASP.NET Core
        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            return cartJson == null ? new List<CartItem>() : JsonConvert.DeserializeObject<List<CartItem>>(cartJson);
        }

        private void SetCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cart));
        }

        public ActionResult Index()
        {
            List<Product> products = new List<Product>();

            try
            {
                using (var client = new ProductServiceClient())
                {
                    products = client.GetAllProducts().ToList();
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Unable to retrieve products: " + ex.Message;
            }

            return View(products);
        }

        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity = 1)
        {
            try
            {
                Product product = null;
                using (var client = new ProductServiceClient())
                {
                    product = client.GetProductById(productId);
                }

                if (product != null)
                {
                    var cart = GetCart();
                    var existingItem = cart.FirstOrDefault(c => c.Product.Id == productId);

                    if (existingItem != null)
                    {
                        existingItem.Quantity += quantity;
                    }
                    else
                    {
                        cart.Add(new CartItem
                        {
                            Product = product,
                            Quantity = quantity
                        });
                    }

                    SetCart(cart);
                    TempData["SuccessMessage"] = product.Name + " has been added to your cart!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Product not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error adding product to cart: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        public ActionResult Cart()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        public ActionResult UpdateQuantity(int productId, int quantity)
        {
            try
            {
                var cart = GetCart();
                var item = cart.FirstOrDefault(c => c.Product.Id == productId);

                if (item != null)
                {
                    if (quantity > 0)
                    {
                        if (quantity <= item.Product.StockQuantity)
                        {
                            item.Quantity = quantity;
                            TempData["SuccessMessage"] = "Quantity updated successfully.";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Requested quantity exceeds available stock (" + item.Product.StockQuantity + " available).";
                        }
                    }
                    else
                    {
                        cart.Remove(item);
                        TempData["SuccessMessage"] = "Item removed from cart.";
                    }

                    SetCart(cart);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating quantity: " + ex.Message;
            }

            return RedirectToAction("Cart");
        }

        [HttpPost]
        public ActionResult RemoveFromCart(int productId)
        {
            try
            {
                var cart = GetCart();
                var item = cart.FirstOrDefault(c => c.Product.Id == productId);

                if (item != null)
                {
                    cart.Remove(item);
                    SetCart(cart);
                    TempData["SuccessMessage"] = item.Product.Name + " has been removed from your cart.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error removing item from cart: " + ex.Message;
            }

            return RedirectToAction("Cart");
        }

        [HttpPost]
        public ActionResult ClearCart()
        {
            SetCart(new List<CartItem>());
            TempData["SuccessMessage"] = "Your cart has been cleared.";
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public ActionResult SubmitOrder()
        {
            try
            {
                var cart = GetCart();

                if (cart == null || !cart.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty. Please add items before submitting an order.";
                    return RedirectToAction("Cart");
                }

                // Calculate order totals
                var subtotal = cart.Sum(item => item.Subtotal);
                var tax = subtotal * 0.08m; // 8% tax
                var shipping = subtotal > 50 ? 0 : 5.99m; // Free shipping over $50
                var total = subtotal + tax + shipping;

                // Create order
                var order = new Order
                {
                    CustomerSessionId = HttpContext.Session.Id,
                    Subtotal = subtotal,
                    Tax = tax,
                    Shipping = shipping,
                    Total = total
                };

                // Add order items
                foreach (var cartItem in cart)
                {
                    order.Items.Add(new OrderItem
                    {
                        ProductId = cartItem.Product.Id,
                        ProductName = cartItem.Product.Name,
                        SKU = cartItem.Product.SKU,
                        Price = cartItem.Product.Price,
                        Quantity = cartItem.Quantity,
                        Subtotal = cartItem.Subtotal
                    });
                }

                // Send order to MSMQ
                var queueService = new OrderQueueService();
                queueService.SendOrder(order);

                // Clear the cart
                SetCart(new List<CartItem>());

                // Redirect to confirmation page
                TempData["SuccessMessage"] = $"Order {order.OrderId} has been submitted successfully! Total: ${total:N2}";
                TempData["OrderId"] = order.OrderId;
                
                return RedirectToAction("OrderConfirmation");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error submitting order: " + ex.Message;
                return RedirectToAction("Cart");
            }
        }

        public ActionResult OrderConfirmation()
        {
            if (TempData["OrderId"] == null)
            {
                return RedirectToAction("Index");
            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
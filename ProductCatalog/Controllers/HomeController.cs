using ProductCatalog.Models;
using ProductCatalog.ProductServiceReference;
using ProductCatalog.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ProductCatalog.Controllers
{
    public class HomeController : Controller
    {
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
                    var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();
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

                    Session["Cart"] = cart;
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
            var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();
            return View(cart);
        }

        [HttpPost]
        public ActionResult UpdateQuantity(int productId, int quantity)
        {
            try
            {
                var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();
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

                    Session["Cart"] = cart;
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
                var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();
                var item = cart.FirstOrDefault(c => c.Product.Id == productId);

                if (item != null)
                {
                    cart.Remove(item);
                    Session["Cart"] = cart;
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
            Session["Cart"] = new List<CartItem>();
            TempData["SuccessMessage"] = "Your cart has been cleared.";
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public ActionResult SubmitOrder()
        {
            try
            {
                var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();

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
                    CustomerSessionId = Session.SessionID,
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

                // Send order to Azure Service Bus
                using (var queueService = new OrderQueueService())
                {
                    queueService.SendOrder(order);
                }

                // Clear the cart
                Session["Cart"] = new List<CartItem>();

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
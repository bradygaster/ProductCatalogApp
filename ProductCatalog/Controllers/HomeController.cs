using ProductCatalog.Models;
using ProductCatalog.Services;
using ProductServiceLibrary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ProductCatalog.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOrderQueueService _queueService;

        public HomeController(IOrderQueueService queueService)
        {
            _queueService = queueService;
        }

        public IActionResult Index()
        {
            List<Product> products = new List<Product>();

            try
            {
                using (var client = new ProductServiceClient())
                {
                    products = client.GetAllProducts();
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Unable to retrieve products: " + ex.Message;
            }

            return View(products);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
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

                    SaveCart(cart);
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

        public IActionResult Cart()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
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

                    SaveCart(cart);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating quantity: " + ex.Message;
            }

            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            try
            {
                var cart = GetCart();
                var item = cart.FirstOrDefault(c => c.Product.Id == productId);

                if (item != null)
                {
                    cart.Remove(item);
                    SaveCart(cart);
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
        public IActionResult ClearCart()
        {
            SaveCart(new List<CartItem>());
            TempData["SuccessMessage"] = "Your cart has been cleared.";
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult SubmitOrder()
        {
            try
            {
                var cart = GetCart();

                if (cart == null || !cart.Any())
                {
                    TempData["ErrorMessage"] = "Your cart is empty. Please add items before submitting an order.";
                    return RedirectToAction("Cart");
                }

                var subtotal = cart.Sum(item => item.Subtotal);
                var tax = subtotal * 0.08m;
                var shipping = subtotal > 50 ? 0 : 5.99m;
                var total = subtotal + tax + shipping;

                var order = new Order
                {
                    CustomerSessionId = HttpContext.Session.Id,
                    Subtotal = subtotal,
                    Tax = tax,
                    Shipping = shipping,
                    Total = total
                };

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

                _queueService.SendOrder(order);

                SaveCart(new List<CartItem>());

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

        public IActionResult OrderConfirmation()
        {
            if (TempData["OrderId"] == null)
            {
                return RedirectToAction("Index");
            }

            return View();
        }

        public IActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public IActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson))
                return new List<CartItem>();
            
            return JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString("Cart", cartJson);
        }
    }
}
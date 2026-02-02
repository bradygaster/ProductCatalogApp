using ProductCatalog.Models;
using ProductCatalog.Services;
using Microsoft.AspNetCore.Mvc;

namespace ProductCatalog.Controllers;

public class HomeController : Controller
{
    private readonly ProductServiceClient _productServiceClient;
    private readonly OrderQueueService _orderQueueService;

    public HomeController(ProductServiceClient productServiceClient, OrderQueueService orderQueueService)
    {
        _productServiceClient = productServiceClient;
        _orderQueueService = orderQueueService;
    }

    public async Task<IActionResult> Index()
    {
        List<Product> products = new List<Product>();

        try
        {
            products = await _productServiceClient.GetAllProductsAsync();
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "Unable to retrieve products: " + ex.Message;
        }

        return View(products);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
    {
        try
        {
            var product = await _productServiceClient.GetProductByIdAsync(productId);

            if (product != null)
            {
                var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
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

                HttpContext.Session.SetObjectAsJson("Cart", cart);
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
        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
        return View(cart);
    }

    [HttpPost]
    public IActionResult UpdateQuantity(int productId, int quantity)
    {
        try
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
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

                HttpContext.Session.SetObjectAsJson("Cart", cart);
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
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.Product.Id == productId);

            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
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
        HttpContext.Session.SetObjectAsJson("Cart", new List<CartItem>());
        TempData["SuccessMessage"] = "Your cart has been cleared.";
        return RedirectToAction("Cart");
    }

    [HttpPost]
    public async Task<IActionResult> SubmitOrder()
    {
        try
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

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

            // Send order to Azure Service Bus
            await _orderQueueService.SendOrderAsync(order);

            // Clear the cart
            HttpContext.Session.SetObjectAsJson("Cart", new List<CartItem>());

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
}
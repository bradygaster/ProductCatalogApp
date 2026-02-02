using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Models;
using ProductCatalog.Services;

namespace ProductCatalog.Controllers;

public class HomeController : Controller
{
    private readonly IProductServiceClient _productServiceClient;
    private readonly IOrderQueueService _orderQueueService;

    public HomeController(IProductServiceClient productServiceClient, IOrderQueueService orderQueueService)
    {
        _productServiceClient = productServiceClient;
        _orderQueueService = orderQueueService;
    }

    public async Task<IActionResult> Index()
    {
        List<Product> products = new();

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
                var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
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

                HttpContext.Session.Set("Cart", cart);
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
        var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
        return View(cart);
    }

    [HttpPost]
    public IActionResult UpdateQuantity(int productId, int quantity)
    {
        try
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
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

                HttpContext.Session.Set("Cart", cart);
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
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.Product.Id == productId);

            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.Set("Cart", cart);
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
        HttpContext.Session.Set("Cart", new List<CartItem>());
        TempData["SuccessMessage"] = "Your cart has been cleared.";
        return RedirectToAction("Cart");
    }

    [HttpPost]
    public async Task<IActionResult> SubmitOrder()
    {
        try
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();

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

            await _orderQueueService.SendOrderAsync(order);

            HttpContext.Session.Set("Cart", new List<CartItem>());

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
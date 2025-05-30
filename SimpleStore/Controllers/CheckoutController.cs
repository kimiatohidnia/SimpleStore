using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SimpleStore.Models;
using SimpleStore.Data;

namespace SimpleStore.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(ApplicationDbContext context, ILogger<CheckoutController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            if (cart.Items.Count == 0)
            {
                TempData["ErrorMessage"] = "Your cart is empty. Please add items before checkout.";
                return RedirectToAction("Index", "Cart");
            }

            var checkoutViewModel = new CheckoutViewModel
            {
                CartItems = cart.Items,
                TotalPrice = cart.TotalPrice
            };

            return View(checkoutViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            _logger.LogInformation("PlaceOrder method called");

            // Add this debugging code
            _logger.LogInformation($"Model.CartItems is null: {model.CartItems == null}");
            _logger.LogInformation($"Model.CartItems count: {model.CartItems?.Count ?? 0}");
            _logger.LogInformation($"ModelState.IsValid: {ModelState.IsValid}");

            // Log all validation errors
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning($"Validation error: {error.ErrorMessage}");
            }


            _logger.LogInformation("PlaceOrder method called");

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Model validation failed");
                    var cart = GetCartFromSession();
                    model.CartItems = cart.Items;
                    model.TotalPrice = cart.TotalPrice;
                    return View("Index", model);
                }

                var cartViewModel = GetCartFromSession();
                if (cartViewModel.Items.Count == 0)
                {
                    _logger.LogWarning("Cart is empty during order placement");
                    TempData["ErrorMessage"] = "Your cart is empty. Please add items before checkout.";
                    return RedirectToAction("Index", "Cart");
                }

                _logger.LogInformation($"Processing order with {cartViewModel.Items.Count} items");

                var order = new Order
                {
                    FullName = model.FullName,
                    Address = model.Address,
                    City = model.City,
                    PostalCode = model.PostalCode,
                    PaymentMethod = model.PaymentMethod,
                    TotalPrice = cartViewModel.TotalPrice,
                    CreatedAt = DateTime.Now,
                    Status = "Processing"
                };

                // Set UserId from claims if authenticated
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        order.UserId = userId;
                        _logger.LogInformation($"Order assigned to user: {userId}");
                        _logger.LogInformation($"User authenticated: {User.Identity.Name}");
                    }
                    else
                    {
                        _logger.LogWarning("User authenticated but userId is null or empty");
                    }
                }
                else
                {
                    _logger.LogInformation("Processing guest order - user not authenticated");
                }

                // Add order items
                foreach (var item in cartViewModel.Items)
                {
                    var orderItem = new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };
                    order.OrderItems.Add(orderItem);
                    _logger.LogInformation($"Added order item: {item.ProductName} x {item.Quantity}");
                }

                // Save to database
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Order saved successfully with ID: {order.Id}, UserId: {order.UserId}");

                // Verify the order was saved correctly
                var savedOrder = await _context.Orders.FirstOrDefaultAsync(o => o.Id == order.Id);
                if (savedOrder != null)
                {
                    _logger.LogInformation($"Verification: Order {savedOrder.Id} found in database with UserId: {savedOrder.UserId}");
                }
                else
                {
                    _logger.LogError($"Verification failed: Order {order.Id} not found in database after save");
                }

                // Clear the cart
                HttpContext.Session.Remove("Cart");
                _logger.LogInformation("Cart cleared from session");

                // Store order ID for confirmation page
                TempData["OrderId"] = order.Id;
                TempData["SuccessMessage"] = "Your order has been placed successfully!";

                return RedirectToAction(nameof(Confirmation));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while placing order");
                TempData["ErrorMessage"] = "An error occurred while processing your order. Please try again.";

                // Return to checkout form with error
                var cart = GetCartFromSession();
                model.CartItems = cart.Items;
                model.TotalPrice = cart.TotalPrice;
                return View("Index", model);
            }
        }

        public async Task<IActionResult> Confirmation()
        {
            if (TempData["OrderId"] == null)
            {
                _logger.LogWarning("Confirmation page accessed without OrderId");
                return RedirectToAction("Index", "Home");
            }

            int orderId = (int)TempData["OrderId"];
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                _logger.LogWarning($"Order not found with ID: {orderId}");
                return NotFound();
            }

            _logger.LogInformation($"Displaying confirmation for order: {orderId}");
            return View(order);
        }

        private CartViewModel GetCartFromSession()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            return string.IsNullOrEmpty(cartJson)
                ? new CartViewModel()
                : JsonConvert.DeserializeObject<CartViewModel>(cartJson);
        }
    }
}
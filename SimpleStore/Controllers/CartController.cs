using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using SimpleStore.Models;
using SimpleStore.Data; // Assuming you have a DbContext

namespace SimpleStore.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context; // Your database context

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cart
        [Authorize]
        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            return View(cart);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        [Authorize]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            // Find the product
            var product = _context.Products.Find(productId);
            if (product == null)
            {
                return NotFound();
            }

            // Get cart from session
            var cart = GetCartFromSession();

            // Check if product already exists in cart
            var existingItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);

            if (existingItem != null)
            {
                // Update quantity if product already in cart
                existingItem.Quantity += quantity;
            }
            else
            {
                // Add new item to cart
                cart.Items.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    ImageUrl = product.ImageUrl,
                    Quantity = quantity
                });
            }

            // Save cart to session
            SaveCartToSession(cart);

            // Redirect back to where the request came from, or to the cart
            if (Request.Headers["Referer"].ToString() != null)
            {
                return Redirect(Request.Headers["Referer"].ToString());
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/UpdateCart
        [HttpPost]
        [Authorize]
        public IActionResult UpdateCart(int productId, int newQuantity)
        {
            var cart = GetCartFromSession();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item == null)
            {
                return NotFound();
            }

            if (newQuantity <= 0)
            {
                // Remove item if quantity is zero or negative
                cart.Items.Remove(item);
            }
            else
            {
                // Update quantity
                item.Quantity = newQuantity;
            }

            SaveCartToSession(cart);
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/RemoveFromCart
        [HttpPost]
        [Authorize]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCartFromSession();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                cart.Items.Remove(item);
                SaveCartToSession(cart);
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper methods for cart session management
        private CartViewModel GetCartFromSession()
        {
            var cartJson = HttpContext.Session.GetString("Cart");

            if (string.IsNullOrEmpty(cartJson))
            {
                return new CartViewModel();
            }

            return JsonConvert.DeserializeObject<CartViewModel>(cartJson);
        }

        private void SaveCartToSession(CartViewModel cart)
        {
            var cartJson = JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString("Cart", cartJson);
        }
    }
}
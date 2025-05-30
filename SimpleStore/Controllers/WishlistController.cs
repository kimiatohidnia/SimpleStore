using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleStore.Data;
using SimpleStore.Models;
using System.Security.Claims;

namespace SimpleStore.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WishlistController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Wishlist
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var wishlist = await GetOrCreateWishlistAsync(userId.Value);
            var wishlistViewModel = await CreateWishlistViewModelAsync(wishlist);

            return View(wishlistViewModel);
        }

        // POST: Add to Wishlist
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Please login to add items to wishlist." });
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null || !product.IsActive)
            {
                return Json(new { success = false, message = "Product not found." });
            }

            var wishlist = await GetOrCreateWishlistAsync(userId.Value);

            // Check if product already exists in wishlist
            var existingItem = await _context.WishlistItems
                .FirstOrDefaultAsync(wi => wi.WishlistId == wishlist.Id && wi.ProductId == productId);

            if (existingItem != null)
            {
                return Json(new { success = false, message = "Product is already in your wishlist." });
            }

            // Add product to wishlist
            var wishlistItem = new WishlistItem
            {
                WishlistId = wishlist.Id,
                ProductId = productId,
                AddedAt = DateTime.UtcNow
            };

            _context.WishlistItems.Add(wishlistItem);
            await _context.SaveChangesAsync();

            // Get updated count
            var itemCount = await _context.WishlistItems.CountAsync(wi => wi.WishlistId == wishlist.Id);

            return Json(new { success = true, message = "Product added to wishlist!", itemCount = itemCount });
        }

        // POST: Remove from Wishlist
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "User not authenticated." });
            }

            var wishlist = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId.Value);

            if (wishlist == null)
            {
                return Json(new { success = false, message = "Wishlist not found." });
            }

            var wishlistItem = await _context.WishlistItems
                .FirstOrDefaultAsync(wi => wi.WishlistId == wishlist.Id && wi.ProductId == productId);

            if (wishlistItem == null)
            {
                return Json(new { success = false, message = "Item not found in wishlist." });
            }

            _context.WishlistItems.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            // Get updated count
            var itemCount = await _context.WishlistItems.CountAsync(wi => wi.WishlistId == wishlist.Id);

            return Json(new { success = true, message = "Product removed from wishlist!", itemCount = itemCount });
        }

        // POST: Move to Cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveToCart(int productId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "User not authenticated." });
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null || !product.IsActive)
            {
                return Json(new { success = false, message = "Product not found." });
            }

            // Add to cart (reuse existing cart logic from ProductController)
            AddProductToCart(product, 1);

            // Remove from wishlist
            var wishlist = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId.Value);

            if (wishlist != null)
            {
                var wishlistItem = await _context.WishlistItems
                    .FirstOrDefaultAsync(wi => wi.WishlistId == wishlist.Id && wi.ProductId == productId);

                if (wishlistItem != null)
                {
                    _context.WishlistItems.Remove(wishlistItem);
                    await _context.SaveChangesAsync();
                }
            }

            return Json(new { success = true, message = "Product moved to cart!" });
        }

        // GET: Check if product is in wishlist (for AJAX calls)
        [HttpGet]
        public async Task<IActionResult> IsInWishlist(int productId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { inWishlist = false });
            }

            var wishlist = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId.Value);

            if (wishlist == null)
            {
                return Json(new { inWishlist = false });
            }

            var exists = await _context.WishlistItems
                .AnyAsync(wi => wi.WishlistId == wishlist.Id && wi.ProductId == productId);

            return Json(new { inWishlist = exists });
        }

        // GET: Get wishlist count (for header badge)
        [HttpGet]
        public async Task<IActionResult> GetWishlistCount()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { count = 0 });
            }

            var wishlist = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId.Value);

            if (wishlist == null)
            {
                return Json(new { count = 0 });
            }

            var count = await _context.WishlistItems.CountAsync(wi => wi.WishlistId == wishlist.Id);
            return Json(new { count = count });
        }

        // Helper Methods
        private int? GetCurrentUserId()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdString, out var userId) ? userId : null;
        }

        private async Task<Wishlist> GetOrCreateWishlistAsync(int userId)
        {
            var wishlist = await _context.Wishlists
                .Include(w => w.WishlistItems)
                .ThenInclude(wi => wi.Product)
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wishlist == null)
            {
                wishlist = new Wishlist
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();

                // Reload with includes
                wishlist = await _context.Wishlists
                    .Include(w => w.WishlistItems)
                    .ThenInclude(wi => wi.Product)
                    .FirstOrDefaultAsync(w => w.Id == wishlist.Id);
            }

            return wishlist;
        }

        private async Task<WishlistViewModel> CreateWishlistViewModelAsync(Wishlist wishlist)
        {
            var viewModel = new WishlistViewModel
            {
                Id = wishlist.Id,
                UserId = wishlist.UserId
            };

            foreach (var item in wishlist.WishlistItems.Where(wi => wi.Product.IsActive))
            {
                viewModel.Items.Add(new WishlistItemViewModel
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    ProductDescription = item.Product.Description,
                    Price = item.Product.Price,
                    ImageUrl = item.Product.ImageUrl,
                    Category = item.Product.Category,
                    IsAvailable = item.Product.Stock > 0,
                    AddedAt = item.AddedAt
                });
            }

            return viewModel;
        }

        private void AddProductToCart(Product product, int quantity)
        {
            // Get cart from session (reusing logic from ProductController)
            var cartJson = HttpContext.Session.GetString("Cart");
            CartViewModel cart;

            if (string.IsNullOrEmpty(cartJson))
            {
                cart = new CartViewModel();
            }
            else
            {
                cart = Newtonsoft.Json.JsonConvert.DeserializeObject<CartViewModel>(cartJson);
            }

            // Check if product already exists in cart
            var existingItem = cart.Items.FirstOrDefault(item => item.ProductId == product.Id);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
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
            var updatedCartJson = Newtonsoft.Json.JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString("Cart", updatedCartJson);
        }
    }
}
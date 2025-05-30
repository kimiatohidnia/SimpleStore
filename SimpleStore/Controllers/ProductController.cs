using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SimpleStore.Data;
using SimpleStore.Models;

namespace SimpleStore.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly int _pageSize = 8;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Product
        public async Task<IActionResult> Index(ProductSearchViewModel searchModel)
        {
            // Start with all products that are active
            var productsQuery = _context.Products.Where(p => p.IsActive);

            // Apply search if provided
            if (!string.IsNullOrEmpty(searchModel.SearchTerm))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Name.Contains(searchModel.SearchTerm)); /* ||*/
                    //p.Description.Contains(searchModel.SearchTerm));
            }

            // Apply category filter if provided
            if (!string.IsNullOrEmpty(searchModel.Category))
            {
                productsQuery = productsQuery.Where(p => p.Category == searchModel.Category);
            }

            // Apply sorting
            switch (searchModel.SortOrder)
            {
                case "price_asc":
                    productsQuery = productsQuery.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    productsQuery = productsQuery.OrderByDescending(p => p.Price);
                    break;
                default:
                    // Default sort by newest
                    productsQuery = productsQuery.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            // Calculate pagination info
            var totalItems = await productsQuery.CountAsync();

            // Ensure page number is valid
            if (searchModel.PageNumber < 1)
            {
                searchModel.PageNumber = 1;
            }

            // Get the products for the current page
            var products = await productsQuery
                .Skip((searchModel.PageNumber - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();

            // Get all unique categories for the filter dropdown
            var categories = await _context.Products
                .Where(p => p.IsActive)
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            ViewBag.Categories = categories;

            // Create the view model
            var viewModel = new ProductListViewModel
            {
                Products = products,
                PaginationInfo = new PaginationInfo
                {
                    CurrentPage = searchModel.PageNumber,
                    ItemsPerPage = _pageSize,
                    TotalItems = totalItems
                },
                SearchQuery = searchModel.SearchTerm,
                Category = searchModel.Category,
                SortOrder = searchModel.SortOrder
            };

            return View(viewModel);
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.SearchQuery = "";

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int id, int quantity = 1)
        {
            // Find the product
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            // Get cart from session
            var cart = GetCartFromSession();

            // Check if product already exists in cart
            var existingItem = cart.Items.FirstOrDefault(item => item.ProductId == id);
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

            TempData["Message"] = "Product added to cart successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }

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
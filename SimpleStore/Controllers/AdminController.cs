using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleStore.Data;
using SimpleStore.Models;

namespace SimpleStore.Controllers
{
    // OPTION 1: Use role-based authorization attribute (RECOMMENDED)
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var dashboardModel = new AdminDashboardViewModel();

            try
            {
                // Get basic statistics
                dashboardModel.TotalUsers = await _context.Users.CountAsync();
                dashboardModel.TotalOrders = await _context.Orders.CountAsync();
                dashboardModel.TotalRevenue = await _context.Orders.SumAsync(o => o.TotalPrice);

                // Get low stock products (threshold: 5)
                dashboardModel.LowStockProducts = await _context.Products
                    .Where(p => p.Stock.HasValue && p.Stock < 5 && p.IsActive)
                    .OrderBy(p => p.Stock)
                    .ToListAsync();

                // Get recent orders (last 10)
                dashboardModel.RecentOrders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(10)
                    .ToListAsync();

                // Get top selling products (by quantity sold)
                var topProducts = await _context.OrderItems
                    .GroupBy(oi => oi.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        TotalQuantity = g.Sum(oi => oi.Quantity),
                        ProductName = g.First().ProductName
                    })
                    .OrderByDescending(x => x.TotalQuantity)
                    .Take(5)
                    .ToListAsync();

                var productIds = topProducts.Select(tp => tp.ProductId).ToList();
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToListAsync();

                dashboardModel.TopSellingProducts = topProducts.Select(tp => new TopSellingProductViewModel
                {
                    Product = products.FirstOrDefault(p => p.Id == tp.ProductId),
                    TotalQuantitySold = tp.TotalQuantity
                }).Where(tp => tp.Product != null).ToList();
            }
            catch (Exception ex)
            {
                // Log error if you have logging configured
                TempData["ErrorMessage"] = "Error loading dashboard data: " + ex.Message;
            }

            return View(dashboardModel);
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(users);
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        // GET: Admin/Products
        public async Task<IActionResult> Products()
        {
            var products = await _context.Products
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(products);
        }

        // POST: Admin/ToggleUserAdmin/{id}
        [HttpPost]
        public async Task<IActionResult> ToggleUserAdmin(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Don't allow removing admin rights from yourself
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (id == currentUserId)
            {
                TempData["ErrorMessage"] = "You cannot remove admin rights from yourself.";
                return RedirectToAction("Users");
            }

            user.IsAdmin = !user.IsAdmin;
            user.UpdatedAt = DateTime.UtcNow;

            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = $"User {user.Username} admin status updated to: {(user.IsAdmin ? "Admin" : "Regular User")}";
            return RedirectToAction("Users");
        }
    }

    // View Models for Admin Dashboard
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<Product> LowStockProducts { get; set; } = new List<Product>();
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<TopSellingProductViewModel> TopSellingProducts { get; set; } = new List<TopSellingProductViewModel>();
    }

    public class TopSellingProductViewModel
    {
        public Product Product { get; set; }
        public int TotalQuantitySold { get; set; }
    }
}
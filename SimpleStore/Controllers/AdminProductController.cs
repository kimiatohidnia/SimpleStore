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
    [Authorize]
    public class AdminProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AdminProduct
        public async Task<IActionResult> Index()
        {
            if (!IsUserAdmin())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var products = await _context.Products
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(products);
        }

        // GET: AdminProduct/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsUserAdmin())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: AdminProduct/Create
        public IActionResult Create()
        {
            if (!IsUserAdmin())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            return View();
        }

        // POST: AdminProduct/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Price,ImageUrl,Stock,Category,IsActive")] Product product)
        {
            if (!IsUserAdmin())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.Now;
                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Product created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: AdminProduct/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsUserAdmin())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: AdminProduct/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,ImageUrl,Stock,Category,IsActive,CreatedAt")] Product product)
        {
            if (!IsUserAdmin())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Product updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: AdminProduct/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsUserAdmin())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: AdminProduct/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsUserAdmin())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product != null)
                {
                    // Check if product is used in any orders before deleting
                    var hasOrders = await _context.OrderItems.AnyAsync(oi => oi.ProductId == id);
                    if (hasOrders)
                    {
                        // Instead of deleting, mark as inactive
                        product.IsActive = false;
                        _context.Update(product);
                        TempData["InfoMessage"] = "Product has been deactivated instead of deleted because it's associated with existing orders.";
                    }
                    else
                    {
                        _context.Products.Remove(product);
                        TempData["SuccessMessage"] = "Product deleted successfully!";
                    }
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting product: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        // Helper method to check if current user is admin (same as AdminController)
        private bool IsUserAdmin()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var adminEmails = new[] { "admin@simplestore.com", "administrator@simplestore.com" };

            if (adminEmails.Contains(userEmail, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }

            var username = User.FindFirstValue(ClaimTypes.Name);
            var adminUsernames = new[] { "admin", "administrator" };

            if (adminUsernames.Contains(username, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userId, out int id) && id == 1)
            {
                return true;
            }

            return false;
        }
    }
}
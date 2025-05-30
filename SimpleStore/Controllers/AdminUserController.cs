using System;
using System.ComponentModel.DataAnnotations;
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
    public class AdminUserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminUserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AdminUser
        public async Task<IActionResult> Index()
        {
            if (!IsUserAdmin())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(users);
        }

        // GET: AdminUser/Details/5
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

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            // Get user's order statistics
            var userOrders = await _context.Orders
                .Where(o => o.UserId == id.ToString())
                .Include(o => o.OrderItems)
                .ToListAsync();

            var userDetailsViewModel = new AdminUserDetailsViewModel
            {
                User = user,
                TotalOrders = userOrders.Count,
                TotalSpent = userOrders.Sum(o => o.TotalPrice),
                RecentOrders = userOrders.OrderByDescending(o => o.CreatedAt).Take(5).ToList()
            };

            return View(userDetailsViewModel);
        }

        // GET: AdminUser/Edit/5
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

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var editViewModel = new AdminUserEditViewModel
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return View(editViewModel);
        }

        // POST: AdminUser/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminUserEditViewModel model)
        {
            if (!IsUserAdmin())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _context.Users.FindAsync(id);
                    if (user == null)
                    {
                        return NotFound();
                    }

                    // Check if email is already taken by another user
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == model.Email && u.Id != id);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Email", "Email is already in use by another user.");
                        return View(model);
                    }

                    user.Username = model.Username;
                    user.Email = model.Email;
                    user.UpdatedAt = DateTime.UtcNow;

                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "User updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(model.Id))
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
            return View(model);
        }

        // GET: AdminUser/Delete/5
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

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            // Check if user has orders
            var hasOrders = await _context.Orders.AnyAsync(o => o.UserId == id.ToString());
            ViewBag.HasOrders = hasOrders;

            return View(user);
        }

        // POST: AdminUser/Delete/5
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
                var user = await _context.Users.FindAsync(id);
                if (user != null)
                {
                    // Check if user has orders
                    var hasOrders = await _context.Orders.AnyAsync(o => o.UserId == id.ToString());
                    if (hasOrders)
                    {
                        TempData["ErrorMessage"] = "Cannot delete user because they have existing orders. Consider deactivating the account instead.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Prevent admin from deleting themselves
                    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (int.TryParse(currentUserId, out int currentId) && currentId == id)
                    {
                        TempData["ErrorMessage"] = "You cannot delete your own account.";
                        return RedirectToAction(nameof(Index));
                    }

                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "User deleted successfully!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting user: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
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

    // View Models for Admin User Management
    public class AdminUserDetailsViewModel
    {
        public User User { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public List<Order> RecentOrders { get; set; } = new List<Order>();
    }

    public class AdminUserEditViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
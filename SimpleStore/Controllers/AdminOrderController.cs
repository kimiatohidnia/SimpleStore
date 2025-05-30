using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SimpleStore.Data;
using SimpleStore.Models;

namespace SimpleStore.Controllers
{
    [Authorize]
    public class AdminOrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminOrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AdminOrder
        public async Task<IActionResult> Index(string statusFilter = "")
        {
            if (!IsUserAdmin())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var ordersQuery = _context.Orders
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter))
            {
                ordersQuery = ordersQuery.Where(o => o.Status == statusFilter);
            }

            var orders = await ordersQuery
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            // Get distinct statuses for filter dropdown
            var statuses = await _context.Orders
                .Select(o => o.Status)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            ViewBag.StatusFilter = statusFilter;
            ViewBag.Statuses = new SelectList(statuses);

            return View(orders);
        }

        // GET: AdminOrder/Details/5
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

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Get user information if order has a user ID
            User user = null;
            if (!string.IsNullOrEmpty(order.UserId) && int.TryParse(order.UserId, out int userId))
            {
                user = await _context.Users.FindAsync(userId);
            }

            var orderDetailsViewModel = new AdminOrderDetailsViewModel
            {
                Order = order,
                User = user
            };

            return View(orderDetailsViewModel);
        }

        // GET: AdminOrder/EditStatus/5
        public async Task<IActionResult> EditStatus(int? id)
        {
            if (!IsUserAdmin())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var editStatusViewModel = new AdminOrderEditStatusViewModel
            {
                Id = order.Id,
                Status = order.Status,
                CancellationRequested = order.CancellationRequested,
                ReturnRequested = order.ReturnRequested,
                FullName = order.FullName,
                TotalPrice = order.TotalPrice,
                CreatedAt = order.CreatedAt
            };

            // Define available statuses
            var statuses = new List<string> { "Processing", "Shipped", "Delivered", "Cancelled", "Returned" };
            ViewBag.StatusOptions = new SelectList(statuses, editStatusViewModel.Status);

            return View(editStatusViewModel);
        }

        // POST: AdminOrder/EditStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatus(int id, AdminOrderEditStatusViewModel model)
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
                    var order = await _context.Orders.FindAsync(id);
                    if (order == null)
                    {
                        return NotFound();
                    }

                    order.Status = model.Status;

                    // If admin approves cancellation/return request, clear the flags
                    if (model.Status == "Cancelled" && order.CancellationRequested)
                    {
                        order.CancellationRequested = false;
                    }
                    if (model.Status == "Returned" && order.ReturnRequested)
                    {
                        order.ReturnRequested = false;
                    }

                    _context.Update(order);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Order status updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(model.Id))
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

            var statuses = new List<string> { "Processing", "Shipped", "Delivered", "Cancelled", "Returned" };
            ViewBag.StatusOptions = new SelectList(statuses, model.Status);
            return View(model);
        }

        // POST: AdminOrder/ApproveCancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveCancel(int id)
        {
            if (!IsUserAdmin())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order != null && order.CancellationRequested)
                {
                    order.Status = "Cancelled";
                    order.CancellationRequested = false;
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Order cancellation approved!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error approving cancellation: " + ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // POST: AdminOrder/ApproveReturn/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveReturn(int id)
        {
            if (!IsUserAdmin())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order != null && order.ReturnRequested)
                {
                    order.Status = "Returned";
                    order.ReturnRequested = false;
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Order return approved!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error approving return: " + ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // GET: AdminOrder/Delete/5
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

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: AdminOrder/Delete/5
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
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order != null)
                {
                    // Remove order items first
                    _context.OrderItems.RemoveRange(order.OrderItems);

                    // Remove order
                    _context.Orders.Remove(order);

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Order deleted successfully!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting order: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
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

    // View Models for Admin Order Management
    public class AdminOrderDetailsViewModel
    {
        public Order Order { get; set; }
        public User User { get; set; }
    }

    public class AdminOrderEditStatusViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Status { get; set; }

        public bool CancellationRequested { get; set; }
        public bool ReturnRequested { get; set; }

        // Display only properties
        public string FullName { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
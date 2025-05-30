namespace SimpleStore.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using SimpleStore.Data;
    using SimpleStore.Models;
    using System.Security.Claims;

    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Order/History
        public async Task<IActionResult> History()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderSummary
                {
                    Id = o.Id,
                    CreatedAt = o.CreatedAt,
                    TotalAmount = o.TotalPrice,
                    Status = o.Status,
                    ItemCount = o.OrderItems.Count,
                    CancellationRequested = o.CancellationRequested,
                    ReturnRequested = o.ReturnRequested
                })
                .ToListAsync();

            var viewModel = new OrderHistoryViewModel
            {
                Orders = orders
            };

            return View(viewModel);
        }

        // GET: Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.Orders
                .Where(o => o.Id == id && o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            var viewModel = new OrderDetailViewModel
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                TotalPrice = order.TotalPrice,
                Status = order.Status,
                FullName = order.FullName,
                Address = order.Address,
                City = order.City,
                PostalCode = order.PostalCode,
                PaymentMethod = order.PaymentMethod,
                CancellationRequested = order.CancellationRequested,
                ReturnRequested = order.ReturnRequested,
                Items = order.OrderItems.Select(oi => new OrderItemDetail
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Order/RequestCancellation/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestCancellation(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.Orders
                .Where(o => o.Id == id && o.UserId == userId)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != "Processing")
            {
                TempData["ErrorMessage"] = "Only orders in 'Processing' status can be cancelled.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (order.CancellationRequested)
            {
                TempData["ErrorMessage"] = "Cancellation has already been requested for this order.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.CancellationRequested = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cancellation request submitted successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Order/RequestReturn/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestReturn(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.Orders
                .Where(o => o.Id == id && o.UserId == userId)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != "Delivered")
            {
                TempData["ErrorMessage"] = "Only delivered orders can be returned.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (order.ReturnRequested)
            {
                TempData["ErrorMessage"] = "Return has already been requested for this order.";
                return RedirectToAction(nameof(Details), new { id });
            }

            order.ReturnRequested = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Return request submitted successfully.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}

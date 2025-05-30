using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleStore.Data;

namespace SimpleStore.Views.ViewComponents
{
    public class CategoryNavigationViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CategoryNavigationViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Get all unique categories from active products
            var categories = await _context.Products
                .Where(p => p.IsActive && !string.IsNullOrEmpty(p.Category))
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return View(categories);
        }
    }
}
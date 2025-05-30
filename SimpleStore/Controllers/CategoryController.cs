using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleStore.Data;

namespace SimpleStore.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            try
            {
                // Fetch all distinct categories from the database
                // Assuming you have a Products table with a Category column
                var categories = await _context.Products
                    .Where(p => !string.IsNullOrEmpty(p.Category))
                    .Select(p => p.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                return View(categories);
            }
            catch (Exception ex)
            {
                // Log the exception if you have logging configured
                // _logger.LogError(ex, "Error retrieving categories");

                // Return empty list on error
                return View(new List<string>());
            }
        }

        // Alternative method if you have a dedicated Categories table
        /*
        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                return View(categories);
            }
            catch (Exception ex)
            {
                return View(new List<Category>());
            }
        }
        */
    }
}
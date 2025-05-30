using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleStore.Data;
using SimpleStore.Models;
using SimpleStore.Utilities;

namespace SimpleStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    return View(model);
                }

                // Hash the password
                var (hash, salt) = PasswordHasher.HashNewPassword(model.Password);

                // Create new user
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    IsAdmin = false, // Regular users are NOT admin by default
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Save user to database
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Log in the user after registration
                await SignInUserAsync(user);

                return RedirectToAction("Profile");
            }

            return View(model);
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Find user by email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user != null)
                {
                    // Verify password
                    bool isPasswordValid = PasswordHasher.VerifyPassword(model.Password, user.PasswordHash, user.PasswordSalt);

                    if (isPasswordValid)
                    {
                        await SignInUserAsync(user, model.RememberMe);

                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }

                        return RedirectToAction("Profile");
                    }
                }

                ModelState.AddModelError("", "Invalid login attempt.");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Clear all session data including cart
            HttpContext.Session.Clear();

            // Sign out the authentication cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }

        // GET: Account/Profile
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            // Get userId as string (consistent with OrderController)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Parse to int only for finding the user record
            var userIdInt = int.Parse(userId);
            var user = await _context.Users.FindAsync(userIdInt);

            if (user == null)
            {
                return NotFound();
            }

            // Fetch recent orders for the user (limit to 5 most recent)
            // Use userId as string to match how it's stored in orders
            var recentOrders = await _context.Orders
                .Where(o => o.UserId == userId) // Use string userId directly
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5) // Show only 5 most recent orders on profile
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

            var model = new ProfileViewModel
            {
                Username = user.Username,
                Email = user.Email,
                RecentOrders = recentOrders
            };

            return View(model);
        }

        // POST: Account/Profile
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return NotFound();
                }

                // Check if email is changed and if it already exists
                if (user.Email != model.Email && await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    return View(model);
                }

                // Update user properties
                user.Username = model.Username;
                user.Email = model.Email;
                user.UpdatedAt = DateTime.UtcNow;

                _context.Update(user);
                await _context.SaveChangesAsync();

                // Update authentication cookie with new user info
                await SignInUserAsync(user);

                TempData["StatusMessage"] = "Your profile has been updated successfully.";
                return RedirectToAction(nameof(Profile));
            }

            return View(model);
        }

        // GET: Account/ChangePassword
        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return NotFound();
                }

                // Verify current password
                bool isCurrentPasswordValid = PasswordHasher.VerifyPassword(model.CurrentPassword, user.PasswordHash, user.PasswordSalt);

                if (!isCurrentPasswordValid)
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                    return View(model);
                }

                // Update password
                var (hash, salt) = PasswordHasher.HashNewPassword(model.NewPassword);
                user.PasswordHash = hash;
                user.PasswordSalt = salt;
                user.UpdatedAt = DateTime.UtcNow;

                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["StatusMessage"] = "Your password has been changed successfully.";
                return RedirectToAction(nameof(Profile));
            }

            return View(model);
        }

        // GET: Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Helper method to sign in user
        private async Task SignInUserAsync(User user, bool isPersistent = false)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            // IMPORTANT: Add admin role claim if user is admin
            if (user.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
    }
}
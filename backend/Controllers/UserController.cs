using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace backend.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /User/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /User/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Username already exists");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                Password = HashPassword(model.Password),
                Email = model.Email,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Add a success message
            TempData["SuccessMessage"] = "Registration successful!";

            // Redirect to login page
            return RedirectToAction("Login");
        }

        // GET: /User/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /User/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (user == null || !VerifyPassword(model.Password, user.Password))
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(model);
            }

            // Add success message
            TempData["SuccessMessage"] = "Login successful!";
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username), // Store the username in the claims
                new Claim(ClaimTypes.Email, user.Email),   // Store the email in the claims
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) // Store the user ID in claims
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Sign the user in
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            // Redirect to home page or dashboard
            return RedirectToAction("Dashboard", "User");
        }
        public async Task<IActionResult> Dashboard()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return RedirectToAction("Login"); // Redirect if user not found
            }

            var model = new UserDashboardViewModel
            {
                Username = user.Username,
                Email = user.Email,
                TotalAccounts = await _context.Accounts.CountAsync(a => a.UserId == user.Id),
                TotalBalance = await _context.Accounts.Where(a => a.UserId == user.Id).SumAsync(a => a.Balance),
                RecentTransactions = await _context.Transactions // Fetch recent transactions
                                            .Where(t => t.UserId == user.Id) // Assuming a UserId property exists in Transaction
                                            .OrderByDescending(t => t.Timestamp)
                                            .Take(10) // Get the latest 10 transactions
                                            .ToListAsync() // Convert to list
            };

            return View(model); // Pass the model to the view
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "You have been logged out.";
            return RedirectToAction("Login", "User"); // Redirect to the login page
        }

        private string HashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return $"{Convert.ToBase64String(salt)}:{hashed}";
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 2)
                return false;

            var salt = Convert.FromBase64String(parts[0]);
            var hash = parts[1];

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return hash == hashed;
        }

        private string GenerateAccountNumber()
        {
            return new Random().Next(1000000000, 2147483647).ToString("D10");
        }
    }
}

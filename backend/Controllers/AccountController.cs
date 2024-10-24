using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{

    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult AccountDetailsView()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user's ID
            if (userId == null)
            {
                return Unauthorized();
            }

            // Convert userId to int if needed
            int parsedUserId = int.Parse(userId);
            var account = _context.Accounts.FirstOrDefault(a => a.UserId == parsedUserId);

            if (account == null)
            {
                // If no account exists, redirect to account creation page
                return RedirectToAction("Create", "Account");
            }

            // Populate the AccountDetailsViewModel with data
            var accountDetailsViewModel = new AccountDetailsViewModel
            {
                AccountHolder = account.User.Username, // Assuming the User entity has a 'Name' field
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                AccountType = account.AccountType,
                Status = "Active", // You can set the status based on business logic
                RecentTransactions = _context.Transactions
                    .Where(t => t.FromAccountId == account.Id || t.ToAccountId == account.Id)
                    .OrderByDescending(t => t.Timestamp)
                    .Take(5) // Show the last 5 transactions
                    .ToList()
            };

            // Pass the model to the view
            return View(accountDetailsViewModel);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountModel model)
        {
            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var account = new AccountModel
            {
                UserId = model.UserId,
                AccountNumber = GenerateAccountNumber(),
                Balance = 0,
                AccountType = model.AccountType,
                CreatedAt = DateTime.UtcNow
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Ok(new { AccountId = account.Id, AccountNumber = account.AccountNumber });
        }

        [HttpGet("{accountId}/details")]
        public async Task<IActionResult> AccountDetails(int accountId)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
            {
                return NotFound("Account not found");
            }

            var transactions = await _context.Transactions
                .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
                .OrderByDescending(t => t.Timestamp)
                .Take(10)
                .ToListAsync();

            var model = new AccountDetailsViewModel
            {
                AccountHolder = account.User.Username, // Assuming User.Name is available
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                AccountType = account.AccountType, // You may need to add this field to Account model
                Status = "Active",  // Assuming all accounts are active
                RecentTransactions = transactions
            };

            return View("AccountDetailsView", model);  // Ensure the view is named `AccountDetails.cshtml`
        }


        [HttpGet("{accountId}/balance")]
        public async Task<IActionResult> GetBalance(int accountId)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
            {
                return NotFound("Account not found");
            }

            return Ok(new { Balance = account.Balance });
        }

        [HttpPost("{accountId}/deposit")]
        public async Task<IActionResult> Deposit(int accountId, [FromBody] TransactionModel model)
        {
            if (model.Amount <= 0)
            {
                return BadRequest("Deposit amount must be positive");
            }

            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
            {
                return NotFound("Account not found");
            }

            account.Balance += model.Amount;
            await _context.SaveChangesAsync();

            return Ok(new { NewBalance = account.Balance });
        }

        [HttpPost("{accountId}/withdraw")]
        public async Task<IActionResult> Withdraw(int accountId, [FromBody] TransactionModel model)
        {
            if (model.Amount <= 0)
            {
                return BadRequest("Withdrawal amount must be positive");
            }

            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null)
            {
                return NotFound("Account not found");
            }

            if (account.Balance < model.Amount)
            {
                return BadRequest("Insufficient funds");
            }

            account.Balance -= model.Amount;
            await _context.SaveChangesAsync();

            return Ok(new { NewBalance = account.Balance });
        }

        private string GenerateAccountNumber()
        {
            return new Random().Next(1000000000, 2147483647).ToString("D10");
        }
    }

   
}
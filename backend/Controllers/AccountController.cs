using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        if(account.User == null)
        {
            return BadRequest("User NOt found");
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



    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateAccountModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAccountModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Get the current user's ID
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var parsedUserId = int.Parse(userId);

        // Create the new account
        var account = new AccountModel
        {
            UserId = parsedUserId,
            AccountNumber = GenerateAccountNumber(),
            Balance = model.InitialDeposit ?? 0m, // Use the initial deposit if provided
            AccountType = model.AccountType,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Account created successfully!";
            return RedirectToAction(nameof(AccountDetailsView));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Error creating account. Please try again.");
            return View(model);
        }
    }

    private string GenerateAccountNumber()
    {
        return DateTime.UtcNow.ToString("yyyyMMdd") +
               new Random().Next(10000000, 99999999).ToString();
    }
}

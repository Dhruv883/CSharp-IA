using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountModel model)
        {
            var user = await _context.Users.FindAsync(model.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var account = new Account
            {
                UserId = model.UserId,
                AccountNumber = GenerateAccountNumber(),
                Balance = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Ok(new { AccountId = account.Id, AccountNumber = account.AccountNumber });
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
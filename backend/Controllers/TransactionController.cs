using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Add()
        {
            return View();
        }
        [HttpGet("History")]
        public async Task<IActionResult> History(DateTime? dateFrom, DateTime? dateTo)
        {

            var transactions = await _context.Transactions
                .Where(t => (!dateFrom.HasValue || t.Timestamp >= dateFrom.Value.ToUniversalTime()) && (!dateTo.HasValue || t.Timestamp <= dateTo.Value.ToUniversalTime()))
                .OrderByDescending(t => t.Timestamp)
                .Select(t => new TransactionViewModel
                {
                    TransactionId = t.Id,
                    Type = t.Type,
                    Amount = t.Amount,
                    Date = t.Timestamp,
                    Status = t.Status
                })
                .ToListAsync();

            var model = new TransactionHistoryViewModel
            {
                Transactions = transactions,
                DateFrom = dateFrom,
                DateTo = dateTo
            };

            return View(model);  // Adjust if using Razor Pages
        }



        [HttpPost("transfer")]
        public async Task<IActionResult> TransferMoney([FromBody] TransferModel model)
        {
            // Ensure model validation
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Message = "Invalid transaction data.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Fetch both accounts
                var fromAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.AccountNumber == model.FromAccountId); // Change here
                var toAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.AccountNumber == model.ToAccountId); // Change here

                // Check if accounts exist
                if (fromAccount == null)
                {
                    return NotFound("Source account not found.");
                }
                if (toAccount == null)
                {
                    return NotFound("Destination account not found.");
                }

                // Ensure sufficient balance in fromAccount
                if (fromAccount.Balance < model.Amount)
                {
                    return BadRequest("Insufficient funds in the source account.");
                }

                // Update account balances
                fromAccount.Balance -= model.Amount;
                toAccount.Balance += model.Amount;

                // Fetch UserId from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier); // Assuming UserId is stored as NameIdentifier
                if (userIdClaim == null)
                {
                    return BadRequest("User not found.");
                }

                // Create a new transaction record
                var transactionRecord = new Transaction
                {
                    FromAccountId = model.FromAccountId,
                    ToAccountId = model.ToAccountId,
                    Amount = model.Amount,
                    Timestamp = DateTime.UtcNow,
                    Type = "Transfer",
                    Status = "Completed", // Assuming a 'Status' field
                    UserId = int.Parse(userIdClaim.Value) // Assuming UserId is stored as an int
                };

                _context.Transactions.Add(transactionRecord);

                // Save changes and commit the transaction
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Return a success response
                return Ok(new
                {
                    TransactionId = transactionRecord.Id,
                    Amount = transactionRecord.Amount,
                    Message = "Transfer successful."
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                // Log error details for debugging
                var errorMessage = $"An error occurred: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $". Inner exception: {ex.InnerException.Message}";
                }
                Console.WriteLine(errorMessage);

                // Optionally log the stack trace
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                return StatusCode(500, errorMessage);
            }
        }
    }
}

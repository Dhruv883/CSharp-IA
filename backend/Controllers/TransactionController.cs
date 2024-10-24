using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
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
            //[FromBody] RegisterModel model
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var fromAccount = await _context.Accounts.FindAsync(model.FromAccountId);
                var toAccount = await _context.Accounts.FindAsync(model.ToAccountId);

                if (fromAccount == null || toAccount == null)
                {
                    return NotFound("One or both accounts not found");
                }

                if (fromAccount.Balance < model.Amount)
                {
                    return BadRequest("Insufficient funds");
                }

                fromAccount.Balance -= model.Amount;
                toAccount.Balance += model.Amount;

                var transactionRecord = new Transaction
                {
                    FromAccountId = model.FromAccountId,
                    ToAccountId = model.ToAccountId,
                    Amount = model.Amount,
                    Timestamp = DateTime.UtcNow
                };

                _context.Transactions.Add(transactionRecord);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new { TransactionId = transactionRecord.Id, Amount = transactionRecord.Amount });
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "An error occurred while processing the transaction");
            }
        }
    }
}

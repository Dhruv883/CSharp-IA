using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
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

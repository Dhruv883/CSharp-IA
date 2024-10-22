using System;
using System.Web.Mvc;
using System.Web.Security;
using BankManagement.Models;
using System.Linq;
using System.Collections.Generic;

namespace BankManagement.Controllers
{
    
    //[Authorize]
    public class TransactionController : Controller
    {
        public ActionResult History(DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            // Sample transaction data - replace with actual data from your database
            var transactions = GetSampleTransactions()
                .Where(t => (!dateFrom.HasValue || t.Date >= dateFrom.Value) &&
                           (!dateTo.HasValue || t.Date <= dateTo.Value))
                .ToList();

            var model = new TransactionHistoryViewModel
            {
                Transactions = transactions,
                DateFrom = dateFrom,
                DateTo = dateTo
            };

            return View(model);
        }

        public ActionResult Create()
        {
            return View(new CreateTransactionViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateTransactionViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Process the transaction
                // This is where you'd implement your transaction logic
                if (ProcessTransaction(model))
                {
                    TempData["SuccessMessage"] = "Transaction completed successfully";
                    return RedirectToAction("History");
                }
                ModelState.AddModelError("", "Unable to process transaction. Please try again.");
            }
            return View(model);
        }

        public ActionResult Details(string id)
        {
            // Get transaction details from your database
            var transaction = GetSampleTransactions().FirstOrDefault(t => t.TransactionId == id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            return View(transaction);
        }

        private bool ProcessTransaction(CreateTransactionViewModel model)
        {
            // Implement your transaction processing logic here
            // Verify sufficient funds, process the transaction, update balances, etc.
            return true;
        }

        private List<TransactionViewModel> GetSampleTransactions()
        {
            // Sample data - replace with actual database queries
            return new List<TransactionViewModel>
            {
                new TransactionViewModel
                {
                    TransactionId = "TRX001",
                    Date = DateTime.Now.AddDays(-1),
                    Type = "Credit",
                    Amount = 1000.00m,
                    Status = "Completed"
                },
                new TransactionViewModel
                {
                    TransactionId = "TRX002",
                    Date = DateTime.Now.AddDays(-2),
                    Type = "Debit",
                    Amount = 500.00m,
                    Status = "Completed"
                },
                // Add more sample transactions as needed
            };
        }
    }
}
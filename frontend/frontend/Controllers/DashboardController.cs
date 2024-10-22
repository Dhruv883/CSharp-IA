using System;
using System.Collections.Generic;
using System.Web.Mvc;
using BankManagement.Models;
using BankManagement.ViewModels;

namespace BankManagement.Controllers
{
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            // In a real application, these would come from your service layer
            ViewBag.TotalAccounts = 1234;
            ViewBag.TotalBalance = "1,234,567";
            ViewBag.PendingTransactions = 45;
            ViewBag.AlertCount = 3;

            var dashboardViewModel = new DashboardViewModel
            {
                RecentTransactions = new List<Transaction>
                {
                    new Transaction { Id = "TR001", AccountNumber = "1234567890", Type = "Deposit", Amount = 1000.00m, Date = DateTime.Now, Status = "Completed" },
                    new Transaction { Id = "TR002", AccountNumber = "0987654321", Type = "Withdrawal", Amount = 500.00m, Date = DateTime.Now.AddHours(-2), Status = "Pending" },
                    // Add more sample transactions as needed
                }
            };

            return View(dashboardViewModel);
        }

        public ActionResult Accounts()
        {
            return View();
        }

        public ActionResult Transactions()
        {
            return View();
        }

    }
}
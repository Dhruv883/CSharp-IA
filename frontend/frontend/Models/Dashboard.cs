using BankManagement.Models;
using System;
using System.Collections.Generic;

namespace BankManagement.Models
{
    public class Transaction
    {
        public string Id { get; set; }
        public string AccountNumber { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
    }
}

namespace BankManagement.ViewModels
{
    public class DashboardViewModel
    {
        public List<Transaction> RecentTransactions { get; set; }
    }
}
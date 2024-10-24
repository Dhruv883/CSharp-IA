namespace backend.Models
{
    public class AccountDetailsViewModel
    {
        public string AccountHolder { get; set; }
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public string AccountType { get; set; }
        public string Status { get; set; }
        public List<Transaction> RecentTransactions { get; set; }
    }
}

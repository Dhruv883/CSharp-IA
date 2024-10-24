namespace backend.Models
{
    public class UserDashboardViewModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public int TotalAccounts { get; set; }
        public decimal TotalBalance { get; set; }
        public List<Transaction> RecentTransactions { get; set; } // Assuming Transaction is your transaction model
    }


}

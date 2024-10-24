namespace backend.Models
{
    public class CreateAccountModel
    {
        public string AccountType { get; set; }
        public decimal? InitialDeposit { get; set; } // Optional initial deposit
    }
}
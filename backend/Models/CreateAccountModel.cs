namespace backend.Models
{
    public class CreateAccountModel
    {
        public int UserId { get; set; }
        public required string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public string AccountType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

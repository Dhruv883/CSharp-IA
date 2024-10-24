using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int FromAccountId { get; set; }
        public int ToAccountId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public int UserId { get; set; }
    }
}

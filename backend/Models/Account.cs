using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Account
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public required string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

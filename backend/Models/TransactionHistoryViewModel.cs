namespace backend.Models
{
    public class TransactionHistoryViewModel
    {
        public List<TransactionViewModel> Transactions { get; set; } = new List<TransactionViewModel>();
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class TransactionViewModel
    {
        public int TransactionId { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
    }
}

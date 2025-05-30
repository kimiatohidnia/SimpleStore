namespace SimpleStore.Models
{
    // Models/OrderHistoryViewModel.cs
    public class OrderHistoryViewModel
    {
        public List<OrderSummary> Orders { get; set; } = new List<OrderSummary>();
    }

    public class OrderSummary
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public int ItemCount { get; set; }
        public bool CancellationRequested { get; set; }
        public bool ReturnRequested { get; set; }
    }
}

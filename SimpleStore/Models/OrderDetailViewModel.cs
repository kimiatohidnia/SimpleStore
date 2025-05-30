namespace SimpleStore.Models
{
    public class OrderDetailViewModel
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }

        // Shipping Information
        public string FullName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }

        // Payment Information
        public string PaymentMethod { get; set; }

        // Request flags
        public bool CancellationRequested { get; set; }
        public bool ReturnRequested { get; set; }

        // Order Items
        public List<OrderItemDetail> Items { get; set; } = new List<OrderItemDetail>();

        // Helper properties
        public bool CanCancel => Status == "Processing" && !CancellationRequested;
        public bool CanReturn => Status == "Delivered" && !ReturnRequested;
    }

    public class OrderItemDetail
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }
}

using System.ComponentModel.DataAnnotations;

namespace SimpleStore.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(200)]
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => UnitPrice * Quantity;

        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }
}
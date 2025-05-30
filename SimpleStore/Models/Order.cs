using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleStore.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // Nullable for guest checkout

        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; }

        [Required(ErrorMessage = "Postal code is required")]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Processing";

        // Request flags
        public bool CancellationRequested { get; set; } = false;
        public bool ReturnRequested { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
} 
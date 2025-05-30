using System.ComponentModel.DataAnnotations;

namespace SimpleStore.Models
{
    public class CheckoutViewModel
    {
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

        // For displaying cart summary on checkout page
        public List<CartItem> CartItems { get; set; }
        public decimal TotalPrice { get; set; }
    }
}

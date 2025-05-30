namespace SimpleStore.Models
{
    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal TotalPrice => Items.Sum(item => item.Subtotal);
    }
}

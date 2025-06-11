

namespace QLNhaSach1.Models
{
    public class Cart
    {
        public int CartId { get; set; }

        public int UserId { get; set; }

        public User user { get; set; }

        public List<CartItem> CartItems { get; set; }

        public decimal TotalPrice { get; set; }
    }

    
}
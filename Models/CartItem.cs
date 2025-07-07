namespace QLNhaSach1.Models
{
    public class CartItem
    {
        public int CartItemId { get; set; }   // Đây là khóa chính

        public int CartId { get; set; }
        public Cart Cart { get; set; } = null!;

        public int BookId { get; set; }
        public Book Book { get; set; } = null!;

        public int Quantity { get; set; }
    }
}

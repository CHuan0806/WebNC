using System.Text.Json.Serialization;

namespace QLNhaSach1.Models
{
    public class CartItem
    {
        public int CartItemId { get; set; }   // Đây là khóa chính

        public int CartId { get; set; }
        [JsonIgnore]
        public Cart Cart { get; set; } = null!;

        public int BookId { get; set; }
        [JsonIgnore]
        public Book Book { get; set; } = null!;

        public int Quantity { get; set; }
    }
}

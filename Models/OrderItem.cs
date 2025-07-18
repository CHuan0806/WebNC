using System.Text.Json.Serialization;

namespace QLNhaSach1.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }
        [JsonIgnore]
        public Order Order { get; set; }

        public int BookId { get; set; }
        [JsonIgnore]
        public Book Book { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }

    }
}
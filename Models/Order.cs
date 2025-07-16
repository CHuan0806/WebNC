
namespace QLNhaSach1.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        public int UserId { get; set; }

        public User user { get; set; }

        public List<OrderItem> OrderItems { get; set; }

        public int? DiscountId { get; set; }
        public Discount discount { get; set; }

        public decimal TotalPrice { get; set; }

        public DateTime OrderDate { get; set; }

        public string Status { get; set; } = "Pending"; 
    }
}
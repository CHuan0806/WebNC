using System.ComponentModel.DataAnnotations.Schema;

namespace QLNhaSach1.Models
{
    public class Cart
    {
        public int CartId { get; set; }

        // Quan hệ đến User
        public int UserId { get; set; }     // Không nên để nullable nếu chắc chắn luôn có user
        public User User { get; set; } = null!;

        // Danh sách sản phẩm trong giỏ
        public List<CartItem> CartItems { get; set; } = new();

        // Tổng giá (có thể tính lại khi cần thay vì lưu vào DB)
        [NotMapped]
        public decimal TotalPrice => CartItems.Sum(item => (item.Book?.price ?? 0) * item.Quantity);
    }
}

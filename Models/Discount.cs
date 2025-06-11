
namespace QLNhaSach1.Models
{
    public class Discount
    {
        public int DiscountId { get; set; }

        public String DiscountCode { get; set; }

        public decimal DiscountPercentage { get; set; }

        public decimal MaxDiscountAmount { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public int RemainingCount { get; set; }
    }
}
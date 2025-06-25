
namespace QLNhaSach1.Models
{
    public class Statistics
    {   
        public int StatisticsId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalUsers { get; set; }
        public int TotalBooks { get; set; }
    }
}
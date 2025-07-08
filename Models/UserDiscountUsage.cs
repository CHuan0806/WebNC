using QLNhaSach1.Models;

public class UserDiscountUsage
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int DiscountId { get; set; }
    public DateTime UsedAt { get; set; }
    public User User { get; set; }
    public Discount Discount { get; set; }
}

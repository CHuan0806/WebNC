// Services/DiscountService.cs
using QLNhaSach1.Models;
using System;
using System.Linq;

namespace QLNhaSach1.Services
{
    public class DiscountService
    {
        private readonly AppDbContext _context;

        public DiscountService(AppDbContext context)
        {
            _context = context;
        }

        public DiscountValidationResult ValidateDiscount(string discountCode, Cart cart, int userId)
        {
            var result = new DiscountValidationResult();
            
            if (string.IsNullOrEmpty(discountCode))
            {
                result.IsValid = false;
                return result;
            }

            var discount = _context.Discount.FirstOrDefault(d =>
                d.DiscountCode == discountCode &&
                d.IsActive &&
                d.StartDate <= DateTime.UtcNow &&
                d.EndDate >= DateTime.UtcNow &&
                d.RemainingCount > 0);

            if (discount == null)
            {
                result.ErrorMessage = "Mã giảm giá không hợp lệ hoặc đã hết hạn";
                return result;
            }

            if (_context.UserDiscountUsages.Any(u => u.UserId == userId && u.DiscountId == discount.DiscountId))
            {
                result.ErrorMessage = "Bạn đã sử dụng mã giảm giá này trước đây";
                return result;
            }

            decimal total = cart.CartItems.Sum(ci => ci.Book.price * ci.Quantity);
            result.DiscountAmount = Math.Min(total * discount.DiscountPercentage / 100, discount.MaxDiscountAmount);
            result.DiscountId = discount.DiscountId.Value;
            result.IsValid = true;

            return result;
        }
    }

    public class DiscountValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public decimal DiscountAmount { get; set; }
        public int DiscountId { get; set; }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNhaSach1.Data;
using System.Security.Claims;

public class CartCountViewComponent : ViewComponent
{
    private readonly AppDbContext _context;

    public CartCountViewComponent(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        int totalItems = 0;
        var userIdStr = HttpContext.Session.GetString("UserID");

        if (int.TryParse(userIdStr, out int userId))
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null)
            {
                totalItems = cart.CartItems.Sum(i => i.Quantity);
            }
        }

        return View(totalItems); // Trả về View để dễ tùy chỉnh giao diện
    }
}

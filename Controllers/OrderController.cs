using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNhaSach1.Data;
using QLNhaSach1.Models;
using QLNhaSach1.Service;

public class OrderController : Controller
{
    private readonly AppDbContext _context;
    private readonly PaypalService _paypalService;

    public OrderController(AppDbContext context, PaypalService paypalService)
    {
        _context = context;
        _paypalService = paypalService;
    }

    [HttpGet]
    public IActionResult Checkout()
    {
        var userIdStr = HttpContext.Session.GetString("UserID");
        if (string.IsNullOrEmpty(userIdStr))
        {
            return RedirectToAction("Login", "User", new { returnUrl = Url.Action("Checkout", "Order") });
        }

        int userId = int.Parse(userIdStr);
        var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
        var cart = _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Book)
            .FirstOrDefault(c => c.UserId == userId);

        if (cart == null || !cart.CartItems.Any())
        {
            TempData["Message"] = "Giỏ hàng trống!";
            return RedirectToAction("Index", "Cart");
        }

        decimal total = cart.CartItems.Sum(ci => ci.Book.price * ci.Quantity);
        decimal discountAmount = decimal.TryParse(TempData["DiscountAmount"]?.ToString(), out var temp) ? temp : 0;
        string discountCode = TempData["DiscountCode"]?.ToString();

        // ❗ Chỉ giữ nếu dữ liệu chưa được xoá sau khi đặt hàng
        if (!Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            TempData.Remove("DiscountAmount");
            TempData.Remove("DiscountCode");
        }


        ViewBag.User = user;
        ViewBag.Cart = cart;
        ViewBag.DiscountAmount = discountAmount;
        ViewBag.TotalAfterDiscount = total - discountAmount;
        ViewBag.DiscountCode = discountCode;

        return View();
    }


    [HttpPost]
    public async Task<IActionResult> Checkout(string userName, string email, string phone, string address, string paymentMethod, string discountCode)
    {
        var userIdStr = HttpContext.Session.GetString("UserID");
        if (string.IsNullOrEmpty(userIdStr))
            return RedirectToAction("Login", "User");

        int userId = int.Parse(userIdStr);
        var cart = _context.Carts.Include(c => c.CartItems).ThenInclude(ci => ci.Book)
                                 .FirstOrDefault(c => c.UserId == userId);

        if (cart == null || !cart.CartItems.Any())
        {
            TempData["Message"] = "Giỏ hàng rỗng!";
            return RedirectToAction("Index", "Cart");
        }

        decimal total = cart.CartItems.Sum(ci => ci.Book.price * ci.Quantity);
        Discount discount = null;
        decimal discountAmount = 0;

        if (!string.IsNullOrEmpty(discountCode))
        {
            discount = _context.Discount.FirstOrDefault(d =>
                d.DiscountCode == discountCode &&
                d.IsActive &&
                d.StartDate <= DateTime.UtcNow &&
                d.EndDate >= DateTime.UtcNow &&
                d.RemainingCount > 0);

            if (discount != null)
            {
                bool userUsed = _context.UserDiscountUsages
                    .Any(ud => ud.UserId == userId && ud.DiscountId == discount.DiscountId);

                if (!userUsed)
                {
                    discountAmount = Math.Min(total * discount.DiscountPercentage / 100, discount.MaxDiscountAmount);
                    total -= discountAmount;
                }
                else
                {
                    TempData["Message"] = "Mã giảm giá đã được sử dụng.";
                    return RedirectToAction("Checkout");
                }
            }
            else
            {
                TempData["Message"] = "Mã giảm giá không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction("Checkout");
            }
        }

        if (paymentMethod == "PayPal")
        {
            TempData["Order_UserId"] = userId;
            TempData["Order_UserName"] = userName;
            TempData["Order_Email"] = email;
            TempData["Order_Phone"] = phone;
            TempData["Order_Address"] = address;
            TempData["Order_Total"] = total.ToString();
            TempData["Order_DiscountId"] = discount?.DiscountId?.ToString();
            TempData["DiscountCode"] = null; // Reset
            TempData["DiscountAmount"] = null;
            TempData.Keep();

            var returnUrl = Url.Action("PaypalSuccess", "Order", null, Request.Scheme);
            var cancelUrl = Url.Action("Checkout", "Order", null, Request.Scheme);

            var redirectUrl = await _paypalService.CreateOrder(Math.Round(total / 26121M, 2), returnUrl, cancelUrl);
            return Redirect(redirectUrl);
        }

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            TotalPrice = total,
            DiscountId = discount?.DiscountId,
            OrderItems = cart.CartItems.Select(ci => new OrderItem
            {
                BookId = ci.BookId,
                Quantity = ci.Quantity,
                Price = ci.Book.price
            }).ToList()
        };

        if (discount != null)
        {
            discount.RemainingCount--;
            if (discount.RemainingCount <= 0)
                discount.IsActive = false;

            _context.Discount.Update(discount);
            _context.UserDiscountUsages.Add(new UserDiscountUsage
            {
                UserId = userId,
                DiscountId = discount.DiscountId.Value,
                UsedAt = DateTime.UtcNow
            });
        }

        _context.Orders.Add(order);
        _context.CartItems.RemoveRange(cart.CartItems);
        _context.Carts.Remove(cart);
        _context.SaveChanges();

        // Xoá mã giảm giá sau khi dùng
        TempData["DiscountCode"] = null;
        TempData["DiscountAmount"] = null;

        TempData["Message"] = "Đặt hàng thành công (COD)!";
        TempData.Clear();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult PaypalSuccess()
    {
        int userId = Convert.ToInt32(TempData["Order_UserId"]);
        string name = TempData["Order_UserName"]?.ToString() ?? "";
        string email = TempData["Order_Email"]?.ToString() ?? "";
        string phone = TempData["Order_Phone"]?.ToString() ?? "";
        string address = TempData["Order_Address"]?.ToString() ?? "";

        decimal.TryParse(TempData["Order_Total"]?.ToString(), out decimal total);
        int? discountId = int.TryParse(TempData["Order_DiscountId"]?.ToString(), out var dId) ? dId : (int?)null;

        var cart = _context.Carts
            .Include(c => c.CartItems).ThenInclude(ci => ci.Book)
            .FirstOrDefault(c => c.UserId == userId);

        if (cart == null || !cart.CartItems.Any())
            return RedirectToAction("Index", "Cart");

        if (discountId.HasValue)
        {
            var discount = _context.Discount.FirstOrDefault(d => d.DiscountId == discountId.Value);
            if (discount != null)
            {
                discount.RemainingCount--;
                if (discount.RemainingCount <= 0)
                    discount.IsActive = false;

                _context.Discount.Update(discount);
                _context.UserDiscountUsages.Add(new UserDiscountUsage
                {
                    UserId = userId,
                    DiscountId = discount.DiscountId.Value,
                    UsedAt = DateTime.UtcNow
                });
            }
        }


        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            TotalPrice = total,
            DiscountId = discountId,
            OrderItems = cart.CartItems.Select(ci => new OrderItem
            {
                BookId = ci.BookId,
                Quantity = ci.Quantity,
                Price = ci.Book.price
            }).ToList()
        };

        _context.Orders.Add(order);
        _context.CartItems.RemoveRange(cart.CartItems);
        _context.Carts.Remove(cart);
        _context.SaveChanges();

        // Xoá TempData liên quan mã giảm giá sau khi dùng
        TempData["DiscountCode"] = null;
        TempData["DiscountAmount"] = null;

        TempData["Message"] = "Đặt hàng thành công (PayPal)!";
        TempData.Clear();
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public IActionResult ApplyDiscount(string discountCode)
    {
        var userIdStr = HttpContext.Session.GetString("UserID");
        if (string.IsNullOrEmpty(userIdStr))
            return RedirectToAction("Login", "User");

        int userId = int.Parse(userIdStr);

        var cart = _context.Carts.Include(c => c.CartItems).ThenInclude(ci => ci.Book)
                                 .FirstOrDefault(c => c.UserId == userId);

        if (cart == null || !cart.CartItems.Any())
            return RedirectToAction("Index", "Cart");

        decimal total = cart.CartItems.Sum(ci => ci.Book.price * ci.Quantity);
        decimal discountAmount = 0;
        Discount discount = null;

        if (!string.IsNullOrEmpty(discountCode))
        {
            discount = _context.Discount.FirstOrDefault(d =>
                d.DiscountCode == discountCode &&
                d.IsActive &&
                d.StartDate <= DateTime.UtcNow &&
                d.EndDate >= DateTime.UtcNow &&
                d.RemainingCount > 0);

            if (discount != null)
            {
                bool userUsed = _context.UserDiscountUsages
                    .Any(u => u.UserId == userId && u.DiscountId == discount.DiscountId);

                if (!userUsed)
                {
                    discountAmount = Math.Min(total * discount.DiscountPercentage / 100, discount.MaxDiscountAmount);
                    TempData["DiscountCode"] = discountCode;
                    TempData["DiscountAmount"] = discountAmount.ToString();
                    TempData["DiscountId"] = discount.DiscountId.ToString();
                    TempData.Keep();
                }
                else
                {
                    TempData["Message"] = "Mã không hợp lệ hoặc đã dùng.";
                }
            }
            else
            {
                TempData["Message"] = "Mã không hợp lệ hoặc đã hết hạn.";
            }
        }

        return RedirectToAction("Checkout");
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNhaSach1.Data;
using QLNhaSach1.Models;

public class OrderController : Controller
{
    private readonly AppDbContext _context;
    private readonly PaypalService _paypalService;

    public OrderController(AppDbContext context, PaypalService paypalService)
    {
        _paypalService = paypalService;
        _context = context;
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

        ViewBag.User = user;
        ViewBag.Cart = cart;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Checkout(string userName, string email, string phone, string address, string paymentMethod, string discountCode)
    {
        var userIdStr = HttpContext.Session.GetString("UserID");
        if (string.IsNullOrEmpty(userIdStr))
            return RedirectToAction("Login", "User");

        int userId = int.Parse(userIdStr);

        var cart = _context.Carts
            .Include(c => c.CartItems).ThenInclude(ci => ci.Book)
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

            bool userUsed = _context.UserDiscountUsages
                .Any(ud => ud.UserId == userId && ud.DiscountId == discount.DiscountId);

            if (discount != null && discount.IsActive && !userUsed)
            {
                discountAmount = Math.Min(total * discount.DiscountPercentage / 100, discount.MaxDiscountAmount);
                total -= discountAmount;
            }
            else
            {
                TempData["Message"] = "Mã giảm giá không hợp lệ hoặc đã được sử dụng.";
                return RedirectToAction("Checkout");
            }


            if (discount != null)
            {
                discountAmount = Math.Min(total * discount.DiscountPercentage / 100, discount.MaxDiscountAmount);
                total -= discountAmount;
            }
        }

        var totalUSD = Math.Round(total / 26121M, 2);

        if (paymentMethod == "PayPal")
        {
            TempData["Order_UserId"] = userId;
            TempData["Order_UserName"] = userName;
            TempData["Order_Email"] = email;
            TempData["Order_Phone"] = phone;
            TempData["Order_Address"] = address;
            TempData["Order_Total"] = total.ToString(); // ✅ Lưu dạng chuỗi
            TempData["Order_DiscountId"] = discount?.DiscountId;
            TempData.Keep();

            var returnUrl = Url.Action("PaypalSuccess", "Order", null, Request.Scheme);
            var cancelUrl = Url.Action("Checkout", "Order", null, Request.Scheme);

            var redirectUrl = await _paypalService.CreateOrder(totalUSD, returnUrl, cancelUrl);
            return Redirect(redirectUrl);
        }

        // Nếu chọn COD thì tạo đơn luôn
        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            TotalPrice = total,
            DiscountId = discount?.DiscountId,
            OrderItems = cart.CartItems.Select(ci => new OrderItem
            {
                BookId = ci.BookId,
                Quantity = ci.Quantity
            }).ToList()
        };

        if (discount != null)
        {
            discount.RemainingCount--;
            discount.IsActive = false;
            _context.Discount.Update(discount);

            _context.UserDiscountUsages.Add(new UserDiscountUsage
            {
                UserId = userId,
                DiscountId = discount.DiscountId,
                UsedAt = DateTime.Now
            });
        }

        _context.Orders.Add(order);
        _context.CartItems.RemoveRange(cart.CartItems);
        _context.Carts.Remove(cart);
        _context.SaveChanges();

        TempData["Message"] = "Đặt hàng thành công (COD)!";
        return RedirectToAction("Index", "Home");
    }

    public IActionResult PayWithPaypal()
    {
        decimal.TryParse(TempData["Order_Total"]?.ToString(), out decimal total);

        TempData.Keep(); // giữ lại TempData sau redirect

        string baseUrl = $"{Request.Scheme}://{Request.Host}";
        string returnUrl = $"{baseUrl}/Order/PaypalSuccess";
        string cancelUrl = $"{baseUrl}/Order/Checkout";

        return View(new PaypalPaymentViewModel
        {
            Total = Convert.ToDecimal(total),
            ReturnUrl = returnUrl,
            CancelUrl = cancelUrl
        });
    }

    public IActionResult PaypalSuccess()
    {
        int userId = Convert.ToInt32(TempData["Order_UserId"]);
        string name = TempData["Order_UserName"]?.ToString() ?? "";
        string email = TempData["Order_Email"]?.ToString() ?? "";
        string phone = TempData["Order_Phone"]?.ToString() ?? "";
        string address = TempData["Order_Address"]?.ToString() ?? "";

        var cart = _context.Carts
            .Include(c => c.CartItems).ThenInclude(ci => ci.Book)
            .FirstOrDefault(c => c.UserId == userId);

        if (cart == null || !cart.CartItems.Any())
            return RedirectToAction("Index", "Cart");

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            TotalPrice = cart.CartItems.Sum(ci => ci.Book.price * ci.Quantity),
            OrderItems = cart.CartItems.Select(ci => new OrderItem
            {
                BookId = ci.BookId,
                Quantity = ci.Quantity
            }).ToList()
        };

        _context.Orders.Add(order);
        _context.CartItems.RemoveRange(cart.CartItems);
        _context.Carts.Remove(cart);
        _context.SaveChanges();

        TempData["Message"] = "Đặt hàng thành công (PayPal)!";
        return RedirectToAction("Index", "Home");
    }
    [HttpPost]
    public IActionResult ApplyDiscount(string discountCode)
    {
        var userIdStr = HttpContext.Session.GetString("UserID");
        if (string.IsNullOrEmpty(userIdStr))
            return RedirectToAction("Login", "User");

        int userId = int.Parse(userIdStr);

        var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
        var cart = _context.Carts.Include(c => c.CartItems).ThenInclude(ci => ci.Book)
                                 .FirstOrDefault(c => c.UserId == userId);

        if (cart == null || !cart.CartItems.Any())
            return RedirectToAction("Index", "Cart");

        var total = cart.CartItems.Sum(ci => ci.Book.price * ci.Quantity);
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

            var userUsed = _context.UserDiscountUsages
                .Any(u => u.UserId == userId && u.DiscountId == discount.DiscountId);

            if (discount != null && !userUsed)
            {
                discountAmount = Math.Min(total * discount.DiscountPercentage / 100, discount.MaxDiscountAmount);
            }
            else
            {
                TempData["Message"] = "Mã không hợp lệ hoặc đã dùng.";
            }
        }

        ViewBag.User = user;
        ViewBag.Cart = cart;
        ViewBag.DiscountCode = discountCode;
        ViewBag.DiscountAmount = discountAmount;
        ViewBag.TotalAfterDiscount = total - discountAmount;

        return View("Checkout");
    }

}

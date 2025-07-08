using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNhaSach1.Data;
using QLNhaSach1.Models;

public class OrderController : Controller
{
    private readonly AppDbContext _context;

    public OrderController(AppDbContext context)
    {
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
    public IActionResult Checkout(string userName, string email, string phone, string address, string paymentMethod)
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

        if (paymentMethod == "PayPal")
        {
            // Lưu tạm thông tin đơn hàng vào TempData để dùng sau khi thanh toán xong
            TempData["Order_UserId"] = userId;
            TempData["Order_UserName"] = userName;
            TempData["Order_Email"] = email;
            TempData["Order_Phone"] = phone;
            TempData["Order_Address"] = address;
            TempData["Order_Total"] = cart.CartItems.Sum(ci => ci.Book.price * ci.Quantity);

            return RedirectToAction("PayWithPaypal");
        }

        // Nếu chọn COD thì tạo đơn luôn
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

        TempData["Message"] = "Đặt hàng thành công (COD)!";
        return RedirectToAction("Index", "BookCustomer");
    }

    public IActionResult PayWithPaypal()
    {
        var total = TempData["Order_Total"];
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
        return RedirectToAction("Index", "BookCustomer");
    }
}

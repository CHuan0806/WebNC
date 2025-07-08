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
    public async Task<IActionResult> Checkout(string userName, string email, string phone, string address, string paymentMethod)
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

        var totalUSD = Math.Round(cart.CartItems.Sum(ci => ci.Book.price * ci.Quantity) / 24000M, 2); // giả định 1 USD = 24,000 VNĐ

        if (paymentMethod == "PayPal")
        {
            TempData["Order_UserId"] = userId;
            TempData["Order_UserName"] = userName;
            TempData["Order_Email"] = email;
            TempData["Order_Phone"] = phone;
            TempData["Order_Address"] = address;
            TempData["Order_Total"] = cart.CartItems.Sum(ci => ci.Book.price * ci.Quantity);
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
        return RedirectToAction("Index", "Home");
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
        return RedirectToAction("Index", "Home");
    }
}

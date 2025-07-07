using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNhaSach1.Data;
using QLNhaSach1.Models;

public class CartController : Controller
{
    private readonly AppDbContext _context;

    public CartController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var userIdStr = HttpContext.Session.GetString("UserID");
        if (string.IsNullOrEmpty(userIdStr))
        {
            // Nếu chưa đăng nhập → chuyển về trang đăng nhập
            return RedirectToAction("Login", "User", new { returnUrl = Url.Action("Index", "Cart") });
        }

        int userId = int.Parse(userIdStr);

        var cart = _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Book)
            .FirstOrDefault(c => c.UserId == userId);

        return View(cart);
    }

    [HttpPost]
    public IActionResult AddToCart(int bookId, int quantity)
    {
        var userIdStr = HttpContext.Session.GetString("UserID");
        if (string.IsNullOrEmpty(userIdStr))
        {
            // Nếu chưa đăng nhập → chuyển về trang đăng nhập
            return RedirectToAction("Login", "User", new { returnUrl = Url.Action("Index", "BookCustomer") });
        }

        int userId = int.Parse(userIdStr);

        var cart = _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefault(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart { UserId = userId, CartItems = new List<CartItem>() };
            _context.Carts.Add(cart);
            _context.SaveChanges(); // tạo cart mới trước khi thêm item
        }

        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.BookId == bookId);

        if (cartItem == null)
        {
            cartItem = new CartItem { BookId = bookId, Quantity = quantity };
            cart.CartItems.Add(cartItem);
        }
        else
        {
            cartItem.Quantity += quantity;
        }

        _context.SaveChanges();

        TempData["Message"] = "Đã thêm vào giỏ hàng!";
        return RedirectToAction("Index", "BookCustomer");
    }

    [HttpPost]
    public IActionResult UpdateCart(int cartItemId, int quantity)
    {
        var cartItem = _context.CartItems.Find(cartItemId);
        if (cartItem != null && quantity > 0)
        {
            cartItem.Quantity = quantity;
            _context.SaveChanges();
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult RemoveFromCart(int cartItemId)
    {
        var cartItem = _context.CartItems.Find(cartItemId);
        if (cartItem != null)
        {
            _context.CartItems.Remove(cartItem);
            _context.SaveChanges();
        }
        return RedirectToAction("Index");
    }
}

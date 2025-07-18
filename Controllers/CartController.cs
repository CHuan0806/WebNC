using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNhaSach1.Data;
using QLNhaSach1.Models;

public class CartController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogger<CartController> _logger;
    private readonly CacheService _cacheService;

    public CartController(AppDbContext context, ILogger<CartController> logger, CacheService cacheService)
    {
        _context = context;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<IActionResult> Index()
    {
        var userIdStr = HttpContext.Session.GetString("UserID");
        _logger.LogInformation("UserID from session: {UserID}", userIdStr);

        if (string.IsNullOrEmpty(userIdStr))
        {
            return RedirectToAction("Login", "User", new { returnUrl = Url.Action("Index", "Cart") });
        }

        int userId = int.Parse(userIdStr);
        string cacheKey = $"cart:{userId}";

        var cachedCart = await _cacheService.GetAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedCart))
        {
            var cartFromCache = JsonSerializer.Deserialize<Cart>(cachedCart);

            if (cartFromCache != null && cartFromCache.CartItems != null && cartFromCache.CartItems.Any())
            {
                foreach (var item in cartFromCache.CartItems)
                {
                    item.Book = _context.Books.Find(item.BookId);
                }

                return View(cartFromCache);
            }
        }


        var cart = _context.Carts
            .Include(c => c.CartItems)
            .ThenInclude(ci => ci.Book)
            .FirstOrDefault(c => c.UserId == userId);

        if (cart != null)
        {
            var serializedCart = System.Text.Json.JsonSerializer.Serialize(cart);
            await _cacheService.SetAsync(cacheKey, serializedCart, TimeSpan.FromMinutes(10));
        }

        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int bookId, int quantity)
    {
        var userIdStr = HttpContext.Session.GetString("UserID");
        Console.WriteLine("UserID from session: " + userIdStr);

        if (string.IsNullOrEmpty(userIdStr))
        {
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
            _context.SaveChanges(); // Lưu để có ID trước khi thêm item
        }

        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.BookId == bookId);

        if (cartItem == null)
        {
            var book = _context.Books.Find(bookId);
            if (book == null)
            {
                return NotFound("Sách không tồn tại.");
            }

            cartItem = new CartItem
            {
                BookId = bookId,
                Quantity = quantity,
                Book = book
            };
            cart.CartItems.Add(cartItem);
        }
        else
        {
            cartItem.Quantity += quantity;
        }

        _context.SaveChanges();
        await _cacheService.RemoveAsync($"cart:{userId}");

        TempData["Message"] = "Đã thêm vào giỏ hàng!";
        return RedirectToAction("Index", "BookCustomer");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateCart(int cartItemId, int quantity)
    {
        var userIdStr = HttpContext.Session.GetString("UserID");
        if (string.IsNullOrEmpty(userIdStr))
        {
            return RedirectToAction("Login", "User", new { returnUrl = Url.Action("Index", "Cart") });
        }

        int userId = int.Parse(userIdStr);

        var cartItem = _context.CartItems.Find(cartItemId);
        if (cartItem != null && quantity > 0)
        {
            cartItem.Quantity = quantity;
            _context.SaveChanges();
            await _cacheService.RemoveAsync($"cart:{userId}");
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> RemoveFromCart(int cartItemId)
    {
        var userIdStr = HttpContext.Session.GetString("UserID");
        if (string.IsNullOrEmpty(userIdStr))
        {
            return RedirectToAction("Login", "User", new { returnUrl = Url.Action("Index", "Cart") });
        }

        int userId = int.Parse(userIdStr);

        var cartItem = _context.CartItems.Find(cartItemId);
        if (cartItem != null)
        {
            _context.CartItems.Remove(cartItem);
            _context.SaveChanges();
            await _cacheService.RemoveAsync($"cart:{userId}");
        }

        return RedirectToAction("Index");
    }
}

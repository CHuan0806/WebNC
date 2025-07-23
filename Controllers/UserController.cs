using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNhaSach1.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using QLNhaSach1.Service;

public class UserController : Controller
{
    private readonly AppDbContext _context;
    private readonly CacheService _cacheService;

    public UserController(AppDbContext context, CacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<IActionResult> Index()
    {
        var role = HttpContext.Session.GetString("Role");
        var userIdStr = HttpContext.Session.GetString("UserID");

        if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(userIdStr))
            return RedirectToAction("Login");

        if (role == Role.Admin.ToString())
        {
            string cacheKey = "user_list";
            string cachedUsers = await _cacheService.GetAsync(cacheKey);

            List<User> users;
            if (!string.IsNullOrEmpty(cachedUsers))
            {
                users = JsonSerializer.Deserialize<List<User>>(cachedUsers);
            }
            else
            {
                users = await _context.Users.ToListAsync();
                string serialized = JsonSerializer.Serialize(users);
                await _cacheService.SetAsync(cacheKey, serialized, TimeSpan.FromMinutes(10));
            }

            return View(users);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public IActionResult Register(User user)
    {
        if (!ModelState.IsValid) return View(user);

        if (_context.Users.Any(u => u.Email == user.Email))
        {
            ModelState.AddModelError("Email", "Email đã được đăng ký.");
            return View(user);
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        user.Role = Role.User;

        _context.Users.Add(user);
        _context.SaveChanges();

        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            return View();
        }

        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Role, user.Role.ToString())
    };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("MyCookieAuth", principal);

        HttpContext.Session.SetString("UserID", user.UserId.ToString());
        HttpContext.Session.SetString("UserName", user.UserName);
        HttpContext.Session.SetString("Role", user.Role.ToString());

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "User");
    }


    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult EditProfile()
    {
        var userId = int.Parse(HttpContext.Session.GetString("UserID"));
        var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
        if (user == null) return NotFound();

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> EditProfile(User user)
    {
        if (!ModelState.IsValid) return View(user);

        var userId = int.Parse(HttpContext.Session.GetString("UserID"));
        var existingUser = _context.Users.FirstOrDefault(u => u.UserId == userId);

        existingUser.UserName = user.UserName;
        existingUser.Email = user.Email;
        existingUser.Phone = user.Phone;
        existingUser.Address = user.Address;
        if (!string.IsNullOrEmpty(user.PasswordHash))
        {
            existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        }

        _context.SaveChanges();
        await _cacheService.RemoveAsync("discount:list");
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null) return NotFound();

        return View(user);
    }

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(User user)
    {
        if (!ModelState.IsValid) return View(user);

        if (_context.Users.Any(u => u.Email == user.Email))
        {
            ModelState.AddModelError("Email", "Email đã được đăng ký.");
            return View(user);
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        _context.Users.Add(user);
        _context.SaveChanges();

        await _cacheService.RemoveAsync("discount:list"); // Clear cache

        return RedirectToAction("Index");
    }


    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        return View(user);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = _context.Users.FirstOrDefault(u => u.UserId == id);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        _context.SaveChanges();
        await _cacheService.RemoveAsync("discount:list");
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Update(int id)
    {
        var user = _context.Users.FirstOrDefault(u => u.UserId == id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Update(User user)
    {
        if (!ModelState.IsValid) return View(user);

        var existingUser = _context.Users.FirstOrDefault(u => u.UserId == user.UserId);
        if (existingUser == null) return NotFound();

        existingUser.UserName = user.UserName;
        existingUser.Email = user.Email;
        existingUser.Phone = user.Phone;
        existingUser.Address = user.Address;

        if (!string.IsNullOrEmpty(user.PasswordHash))
        {
            existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        }

        _context.SaveChanges();
        await _cacheService.RemoveAsync("discount:list");
        return RedirectToAction("Index");
    }
}

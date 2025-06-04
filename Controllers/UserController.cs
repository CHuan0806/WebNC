using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNhaSach1.Models;

public class UserController : Controller
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _context.Users.ToListAsync();
        return View(users);
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public IActionResult Register(User user)
    {
        if (!ModelState.IsValid) return View(user);

        if (_context.Users.Any(u => u.Email == user.Email))
        {
            ModelState.AddModelError("Email", "Email đã được đăng ký. ");
            return View(user);
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        user.Role = Role.User;

        _context.Users.Add(user);
        _context.SaveChanges();

        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Login() => View();


    [HttpPost]
    public IActionResult Login(string email, string password)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            return View();
        }

        HttpContext.Session.SetString("UserID", user.UserId.ToString());
        HttpContext.Session.SetString("UserName", user.UserName);
        HttpContext.Session.SetString("Role", user.Role.ToString());

        if (user.Role == Role.Admin)
        {
            return RedirectToAction("Index", "Admin");
        }
        else
        {
            return RedirectToAction("Index", "User");
        }
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult EditProfile() => View();

    [HttpPut]
    public IActionResult EditProfile(User user)
    {
        if (!ModelState.IsValid) return View();

        var userId = int.Parse(HttpContext.Session.GetString("UserID"));
        var existingUser = _context.Users.FirstOrDefault(u => u.UserId == userId);

        existingUser.UserName = user.UserName;
        existingUser.Email = user.Email;
        existingUser.Phone = user.Phone;
        existingUser.Address = user.Address;

        _context.SaveChanges();
        return RedirectToAction("Index");
    }
}
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
        var role = HttpContext.Session.GetString("Role");
        var userIdStr = HttpContext.Session.GetString("UserID");

        if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(userIdStr))
            return RedirectToAction("Login");

        if (role == Role.Admin.ToString())
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
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
    public IActionResult Login(string email, string password, string? returnUrl)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !user.PasswordHash.StartsWith("$2"))
        {
            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            return View();
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            return View();
        }

        HttpContext.Session.SetString("UserID", user.UserId.ToString());
        HttpContext.Session.SetString("UserName", user.UserName);
        HttpContext.Session.SetString("Role", user.Role.ToString());

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

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
    public IActionResult EditProfile(User user)
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
    public IActionResult Create(User user)
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
    public IActionResult DeleteConfirmed(int id)
    {
        var user = _context.Users.FirstOrDefault(u => u.UserId == id);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        _context.SaveChanges();
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
    public IActionResult Update(User user)
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
        return RedirectToAction("Index");
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNhaSach1.Models;
using System.Threading.Tasks;

public class CategoryController : Controller
{
    private readonly AppDbContext _context;

    public CategoryController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories.ToListAsync();
        return View(categories);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();
        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Category category)
    {
        if (ModelState.IsValid)
        {
            var exists = await _context.Categories.AnyAsync(c => c.CategoryId == category.CategoryId);
            if (!exists) return NotFound();

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();
        return View(category);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return NotFound();

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> BooksByCategory(int id)
    {
        // Lấy category kèm sách (nếu cần hiển thị tên danh mục)
        var category = await _context.Categories
                                     .Include(c => c.Books)
                                     .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category == null)
        {
            return NotFound();
        }

        // Lấy danh sách sách theo category
        var books = category.Books;

        // Truyền tên category vào ViewData hoặc ViewBag để hiển thị tiêu đề
        ViewBag.categoryName = category.categoryName;

        return View(books);
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLNhaSach1.Models;

public class BookController : Controller
{
    private readonly AppDbContext _context;

    public BookController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var books = await _context.Books.Include(b => b.Category).ToListAsync();
        return View(books);
    }

    public IActionResult Create()
    {
        // ✅ Sử dụng SelectList thay vì List<Category>
        ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "categoryName");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Book book)
    {
        if (ModelState.IsValid)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        foreach (var key in ModelState.Keys)
        {
            var state = ModelState[key];
            foreach (var error in state.Errors)
            {
                Console.WriteLine($"❌ Field: {key} - Error: {error.ErrorMessage}");
            }
        }

        // ✅ Truyền lại SelectList nếu ModelState bị lỗi
        ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "categoryName", book.CategoryId);
        return View(book);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null) return NotFound();

        ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "categoryName", book.CategoryId);
        return View(book);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Book book)
    {
        if (ModelState.IsValid)
        {
            _context.Update(book);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "categoryName", book.CategoryId);
        return View(book);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var book = await _context.Books.FindAsync(id);
        return View(book);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var book = await _context.Books.FindAsync(id);
        _context.Books.Remove(book); // ⚠️ CS8604

        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Details(int id)
    {
        var book = await _context.Books.Include(b => b.Category)
                                       .FirstOrDefaultAsync(b => b.bookId == id);
        if (book == null) return NotFound();
        return View(book);
    }
}

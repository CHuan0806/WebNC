using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLNhaSach1.Models;
using QLNhaSach1.Models.ViewModels;
using QLNhaSach1.Service;

public class BookController : Controller
{
    private readonly AppDbContext _context;
    private readonly CacheService _cacheService;

    public BookController(AppDbContext context, CacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }
    public async Task<IActionResult> Index()
    {
        string cacheKey = "book:list:";

        var cachedBooks = await _cacheService.GetAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedBooks))
        {
            var booksFromCache = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Book>>(cachedBooks);
            return View(booksFromCache);
        }

        var books = await _context.Books.Include(b => b.Category).ToListAsync();
        await _cacheService.SetAsync(cacheKey, Newtonsoft.Json.JsonConvert.SerializeObject(books), TimeSpan.FromMinutes(10));
        return View(books);
    }

    public async Task<IActionResult> Create()
    {
        var categories = await _context.Categories.ToListAsync();
        var viewModel = new BookViewModel
        {
            Categories = new SelectList(categories, "CategoryId", "categoryName")
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(BookViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            _context.Books.Add(viewModel.Book);
            await _context.SaveChangesAsync();
            await _cacheService.RemoveAsync("discount:list");

            return RedirectToAction(nameof(Index));
        }

        // Nếu lỗi, load lại danh mục
        var categories = await _context.Categories.ToListAsync();
        viewModel.Categories = new SelectList(categories, "CategoryId", "categoryName", viewModel.Book.CategoryId);
        return View(viewModel);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null) return NotFound();

        var categories = await _context.Categories.ToListAsync();
        var viewModel = new BookViewModel
        {
            Book = book,
            Categories = new SelectList(categories, "CategoryId", "categoryName", book.CategoryId)
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(BookViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            _context.Update(viewModel.Book);
            await _context.SaveChangesAsync();
            await _cacheService.RemoveAsync("discount:list");

            return RedirectToAction(nameof(Index));
        }

        var categories = await _context.Categories.ToListAsync();
        viewModel.Categories = new SelectList(categories, "CategoryId", "categoryName", viewModel.Book.CategoryId);
        return View(viewModel);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var book = await _context.Books.Include(b => b.Category).FirstOrDefaultAsync(b => b.bookId == id);
        return book == null ? NotFound() : View(book);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book != null)
        {
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            await _cacheService.RemoveAsync("discount:list");

        }
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var book = await _context.Books.Include(b => b.Category).FirstOrDefaultAsync(b => b.bookId == id);
        return book == null ? NotFound() : View(book);
    }
}

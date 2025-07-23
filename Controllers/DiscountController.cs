using Microsoft.AspNetCore.Mvc;
using QLNhaSach1.Models;
using Microsoft.EntityFrameworkCore;
using QLNhaSach1.Service;

public class DiscountController : Controller
{
    private readonly AppDbContext _context;

    private readonly CacheService _cacheService;

    public DiscountController(AppDbContext context, CacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    // GET: /Discount
    public async Task<IActionResult> Index()
    {
        string cacheKey = "discount:list";

        var cachedData = await _cacheService.GetAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            var discounts = System.Text.Json.JsonSerializer.Deserialize<List<Discount>>(cachedData);
            return View(discounts);
        }

        var discountsFromDb = await _context.Discount.ToListAsync();

        var serializedData = System.Text.Json.JsonSerializer.Serialize(discountsFromDb);
        await _cacheService.SetAsync(cacheKey, serializedData, TimeSpan.FromMinutes(10));

        return View(discountsFromDb);
    }


    // GET: /Discount/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Discount/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Discount discount)
    {
        if (ModelState.IsValid)
        {
            discount.IsActive = true;
            discount.StartDate = DateTime.SpecifyKind(discount.StartDate, DateTimeKind.Utc);
            discount.EndDate = DateTime.SpecifyKind(discount.EndDate, DateTimeKind.Utc);

            _context.Discount.Add(discount);
            _context.SaveChanges();
            await _cacheService.RemoveAsync("discount:list");
            return RedirectToAction(nameof(Index));
        }
        return View(discount);
    }

    // GET: /Discount/Edit/5
    public IActionResult Edit(int id)
    {
        var discount = _context.Discount.Find(id);
        if (discount == null)
        {
            return NotFound();
        }
        return View(discount);
    }

    // POST: /Discount/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Discount discount)
    {
        if (id != discount.DiscountId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            discount.StartDate = DateTime.SpecifyKind(discount.StartDate, DateTimeKind.Utc);
            discount.EndDate = DateTime.SpecifyKind(discount.EndDate, DateTimeKind.Utc);

            _context.Entry(discount).State = EntityState.Modified;
            _context.SaveChanges();
            await _cacheService.RemoveAsync("discount:list");
            return RedirectToAction(nameof(Index));
        }
        return View(discount);
    }

    // GET: /Discount/Delete/5
    public IActionResult Delete(int id)
    {
        var discount = _context.Discount.Find(id);
        if (discount == null)
        {
            return NotFound();
        }
        return View(discount);
    }

    // POST: /Discount/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var discount = _context.Discount.Find(id);
        if (discount != null)
        {
            _context.Discount.Remove(discount);
            _context.SaveChanges();
            await _cacheService.RemoveAsync("discount:list");
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: /Discount/Details/5
    public IActionResult Details(int id)
    {
        var discount = _context.Discount.Find(id);
        if (discount == null)
        {
            return NotFound();
        }
        return View(discount);
    }

    // POST: /Discount/Deactivate/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var discount = _context.Discount.Find(id);
        if (discount == null)
        {
            return NotFound();
        }

        discount.IsActive = false;
        _context.Update(discount);
        await _context.SaveChangesAsync();

        await _cacheService.RemoveAsync("discount:list");

        return RedirectToAction(nameof(Index));
    }

}

using Microsoft.AspNetCore.Mvc;
using QLNhaSach1.Models;
using Microsoft.EntityFrameworkCore;

public class DiscountController : Controller
{
    private readonly AppDbContext _context;

    public DiscountController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Discount
    public IActionResult Index()
    {
        var discounts = _context.Discount.ToList();
        return View(discounts);
    }

    // GET: /Discount/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Discount/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Discount discount)
    {
        if (ModelState.IsValid)
        {
            discount.IsActive = true;
            discount.StartDate = DateTime.SpecifyKind(discount.StartDate, DateTimeKind.Utc);
            discount.EndDate = DateTime.SpecifyKind(discount.EndDate, DateTimeKind.Utc);

            _context.Discount.Add(discount);
            _context.SaveChanges();
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
    public IActionResult Edit(int id, Discount discount)
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
    public IActionResult DeleteConfirmed(int id)
    {
        var discount = _context.Discount.Find(id);
        if (discount != null)
        {
            _context.Discount.Remove(discount);
            _context.SaveChanges();
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
}

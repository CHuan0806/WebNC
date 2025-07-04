using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLNhaSach1.Models;
using QLNhaSach1.ViewModels;

namespace QLNhaSach1.Controllers
{
    public class BookCustomerController : Controller
    {
        private readonly AppDbContext _context;

        public BookCustomerController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? categoryId, string? author, string? sortBy, int? minPrice, int? maxPrice, int page = 1)
        {
            int pageSize = 4; // ✅ Hiển thị 4 sản phẩm mỗi trang

            var query = _context.Books
                .Include(b => b.Category)
                .Where(b => b.bookStatus == true); // chỉ lấy sách còn bán

            // Lọc theo danh mục
            if (categoryId.HasValue)
                query = query.Where(b => b.CategoryId == categoryId.Value);

            // Lọc theo tác giả
            if (!string.IsNullOrEmpty(author))
                query = query.Where(b => b.author == author);

            // ✅ Lọc theo giá
            if (minPrice.HasValue)
                query = query.Where(b => b.price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(b => b.price <= maxPrice.Value);

            // Sắp xếp trước khi phân trang
            switch (sortBy)
            {
                case "name_desc":
                    query = query.OrderByDescending(b => b.bookName);
                    break;
                case "price_asc":
                    query = query.OrderBy(b => b.price);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(b => b.price);
                    break;
                default:
                    query = query.OrderBy(b => b.bookName); // mặc định là name_asc
                    break;
            }

            // Tổng số sản phẩm sau khi lọc
            int totalItems = query.Count();

            // Phân trang
            var books = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Tạo ViewModel
            var viewModel = new BookListViewModel
            {
                Books = books,
                AllCategories = _context.Categories.ToList(),
                AllAuthors = _context.Books
                    .Where(b => b.bookStatus == true)
                    .Select(b => b.author)
                    .Distinct()
                    .ToList(),
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                SelectedCategoryId = categoryId,
                SelectedAuthor = author
            };

            return View(viewModel);
        }
    }
}

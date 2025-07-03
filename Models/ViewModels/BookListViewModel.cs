using QLNhaSach1.Models;

namespace QLNhaSach1.ViewModels
{
    public class BookListViewModel
    {
        public List<Book> Books { get; set; }
        public List<Category> AllCategories { get; set; }
        public List<string> AllAuthors { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        // Nếu cần lọc lại từ view:
        public int? SelectedCategoryId { get; set; }
        public string? SelectedAuthor { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace QLNhaSach1.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [Display(Name = "Tên danh mục")]
        public string categoryName { get; set; }

        // Navigation property
        public List<Book> Books { get; set; } = new List<Book>();
    }
}

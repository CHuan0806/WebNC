using QLNhaSach1.Models;

namespace QLNhaSach1.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            // Nếu chưa có Category thì thêm
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { categoryName = "Văn học" },
                    new Category { categoryName = "Kinh tế" },
                    new Category { categoryName = "Thiếu nhi" }
                };

                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            // Nếu chưa có Book thì thêm
            if (!context.Books.Any())
            {
                var vanHoc = context.Categories.FirstOrDefault(c => c.categoryName == "Văn học");
                if (vanHoc != null)
                {
                    var books = new List<Book>
                    {
                        new Book
                        {
                            bookName = "Dế Mèn Phiêu Lưu Ký",
                            description = "Cuốn truyện thiếu nhi kinh điển của Tô Hoài.",
                            author = "Tô Hoài",
                            bookStatus = true,
                            quantity = 20,
                            price = 50000,
                            CategoryId = vanHoc.CategoryId,
                            ImageUrl = "~/images/dacnhantam86.jpg"
                        },
                        new Book
                        {
                            bookName = "Tuổi Trẻ Đáng Giá Bao Nhiêu",
                            description = "Cuốn sách truyền cảm hứng dành cho giới trẻ.",
                            author = "Rosie Nguyễn",
                            bookStatus = true,
                            quantity = 30,
                            price = 70000,
                            CategoryId = vanHoc.CategoryId,
                            ImageUrl = "~/images/dacnhantam86.jpg"
                        }
                    };

                    context.Books.AddRange(books);
                    context.SaveChanges();
                }
            }
        }
    }
}

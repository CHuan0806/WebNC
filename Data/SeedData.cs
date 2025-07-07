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
                            bookName = "Chiến Binh Cầu Vồng",
                            description = "Chiến binh Cầu vồng đó có đủ sức chinh phục quãng đường ngày ngày đạp xe bốn mươi cây số,",
                            author = "Andrea Hirata",
                            bookStatus = true,
                            quantity = 20,
                            price = 109000,
                            CategoryId = vanHoc.CategoryId,
                            ImageUrl = "~/images/chien-binh-cau-vong.jpg"
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
                            ImageUrl = "~/images/tuoi-tre-dang-gia-bao-nhieu.jpg"
                        }
                    };

                    context.Books.AddRange(books);
                    context.SaveChanges();
                }
            }
        }
    }
}

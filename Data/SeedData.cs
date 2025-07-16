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
                    new Category { categoryName = "Thiếu nhi" },
                    new Category { categoryName = "Kỹ năng sống" },
                    new Category { categoryName = "Công nghệ" },
                    new Category { categoryName = "Lịch sử" },
                    new Category { categoryName = "Tâm lý" }
                };

                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            // Nếu chưa có Book thì thêm
            if (!context.Books.Any())
            {
                var categories = context.Categories.ToList();
                var books = new List<Book>
                {
                    // Văn học (5 sách)
                    new Book
                    {
                        bookName = "Chiến Binh Cầu Vồng",
                        description = "Chiến binh Cầu vồng đó có đủ sức chinh phục quãng đường ngày ngày đạp xe bốn mươi cây số,",
                        author = "Andrea Hirata",
                        bookStatus = true,
                        quantity = 20,
                        price = 109000,
                        CategoryId = categories.First(c => c.categoryName == "Văn học").CategoryId,
                        ImageUrl = "~/images/chien-binh-cau-vong.jpg",
                        Rating = 4.5f,
                        RatingCount = 10
                    },
                    new Book
                    {
                        bookName = "Tuổi Trẻ Đáng Giá Bao Nhiêu",
                        description = "Cuốn sách truyền cảm hứng dành cho giới trẻ.",
                        author = "Rosie Nguyễn",
                        bookStatus = true,
                        quantity = 30,
                        price = 70000,
                        CategoryId = categories.First(c => c.categoryName == "Văn học").CategoryId,
                        ImageUrl = "~/images/tuoi-tre-dang-gia-bao-nhieu.jpg",
                        Rating = 4.8f,
                        RatingCount = 25
                    },
                    new Book
                    {
                        bookName = "Đắc Nhân Tâm",
                        description = "Cuốn sách nổi tiếng về nghệ thuật giao tiếp và ứng xử.",
                        author = "Dale Carnegie",
                        bookStatus = true,
                        quantity = 25,
                        price = 85000,
                        CategoryId = categories.First(c => c.categoryName == "Văn học").CategoryId,
                        ImageUrl = "~/images/dacnhantam86.jpg",
                        Rating = 5.0f,
                        RatingCount = 40
                    },
                    new Book
                    {
                        bookName = "Nhà Giả Kim",
                        description = "Hành trình tìm kiếm kho báu và ý nghĩa cuộc sống.",
                        author = "Paulo Coelho",
                        bookStatus = true,
                        quantity = 18,
                        price = 75000,
                        CategoryId = categories.First(c => c.categoryName == "Văn học").CategoryId,
                        ImageUrl = "~/images/product-item6.jpg",
                        Rating = 4.2f,
                        RatingCount = 8
                    },
                    new Book
                    {
                        bookName = "Cách Nghĩ Để Thành Công",
                        description = "Bí quyết thành công từ tư duy tích cực.",
                        author = "Napoleon Hill",
                        bookStatus = true,
                        quantity = 22,
                        price = 95000,
                        CategoryId = categories.First(c => c.categoryName == "Văn học").CategoryId,
                        ImageUrl = "~/images/product-item7.jpg",
                        Rating = 4.0f,
                        RatingCount = 5
                    },
                    // Kinh tế (5 sách)
                    new Book
                    {
                        bookName = "Cha Giàu Cha Nghèo",
                        description = "Bí quyết làm giàu từ tư duy tài chính.",
                        author = "Robert Kiyosaki",
                        bookStatus = true,
                        quantity = 15,
                        price = 120000,
                        CategoryId = categories.First(c => c.categoryName == "Kinh tế").CategoryId,
                        ImageUrl = "~/images/product-item1.jpg",
                        Rating = 4.7f,
                        RatingCount = 12
                    },
                    new Book
                    {
                        bookName = "Người Giàu Có Nhất Thành Babylon",
                        description = "Bài học tài chính cá nhân từ thành phố cổ Babylon.",
                        author = "George S. Clason",
                        bookStatus = true,
                        quantity = 18,
                        price = 95000,
                        CategoryId = categories.First(c => c.categoryName == "Kinh tế").CategoryId,
                        ImageUrl = "~/images/product-item2.jpg",
                        Rating = 4.9f,
                        RatingCount = 30
                    },
                    new Book
                    {
                        bookName = "Nghĩ Giàu Làm Giàu",
                        description = "13 nguyên tắc nghĩ giàu làm giàu.",
                        author = "Napoleon Hill",
                        bookStatus = true,
                        quantity = 12,
                        price = 110000,
                        CategoryId = categories.First(c => c.categoryName == "Kinh tế").CategoryId,
                        ImageUrl = "~/images/product-item8.jpg",
                        Rating = 4.5f,
                        RatingCount = 15
                    },
                    new Book
                    {
                        bookName = "Bí Mật Tư Duy Triệu Phú",
                        description = "Khám phá tư duy của những người giàu có.",
                        author = "T. Harv Eker",
                        bookStatus = true,
                        quantity = 16,
                        price = 105000,
                        CategoryId = categories.First(c => c.categoryName == "Kinh tế").CategoryId,
                        ImageUrl = "~/images/product-item1.jpg",
                        Rating = 4.8f,
                        RatingCount = 20
                    },
                    new Book
                    {
                        bookName = "Đầu Tư Thông Minh",
                        description = "Hướng dẫn đầu tư cho người mới bắt đầu.",
                        author = "Benjamin Graham",
                        bookStatus = true,
                        quantity = 14,
                        price = 125000,
                        CategoryId = categories.First(c => c.categoryName == "Kinh tế").CategoryId,
                        ImageUrl = "~/images/product-item2.jpg",
                        Rating = 4.6f,
                        RatingCount = 18
                    },
                    // Thiếu nhi (4 sách)
                    new Book
                    {
                        bookName = "Dế Mèn Phiêu Lưu Ký",
                        description = "Tác phẩm thiếu nhi kinh điển của Tô Hoài.",
                        author = "Tô Hoài",
                        bookStatus = true,
                        quantity = 22,
                        price = 60000,
                        CategoryId = categories.First(c => c.categoryName == "Thiếu nhi").CategoryId,
                        ImageUrl = "~/images/de-men-50k_1.JPG",
                        Rating = 4.9f,
                        RatingCount = 25
                    },
                    new Book
                    {
                        bookName = "Hoàng Tử Bé",
                        description = "Tác phẩm văn học thiếu nhi nổi tiếng thế giới.",
                        author = "Antoine de Saint-Exupéry",
                        bookStatus = true,
                        quantity = 20,
                        price = 65000,
                        CategoryId = categories.First(c => c.categoryName == "Thiếu nhi").CategoryId,
                        ImageUrl = "~/images/product-item3.jpg",
                        Rating = 4.7f,
                        RatingCount = 15
                    },
                    new Book
                    {
                        bookName = "Alice Ở Xứ Sở Diệu Kỳ",
                        description = "Cuộc phiêu lưu kỳ thú của cô bé Alice.",
                        author = "Lewis Carroll",
                        bookStatus = true,
                        quantity = 18,
                        price = 70000,
                        CategoryId = categories.First(c => c.categoryName == "Thiếu nhi").CategoryId,
                        ImageUrl = "~/images/product-item4.jpg",
                        Rating = 4.8f,
                        RatingCount = 20
                    },
                    new Book
                    {
                        bookName = "Peter Pan",
                        description = "Câu chuyện về cậu bé không bao giờ lớn.",
                        author = "J.M. Barrie",
                        bookStatus = true,
                        quantity = 16,
                        price = 75000,
                        CategoryId = categories.First(c => c.categoryName == "Thiếu nhi").CategoryId,
                        ImageUrl = "~/images/product-item5.jpg",
                        Rating = 4.6f,
                        RatingCount = 12
                    },
                    // Kỹ năng sống (4 sách)
                    new Book
                    {
                        bookName = "Phát Triển Kỹ Năng Lãnh Đạo",
                        description = "Cẩm nang phát triển kỹ năng lãnh đạo cá nhân.",
                        author = "John C. Maxwell",
                        bookStatus = true,
                        quantity = 10,
                        price = 110000,
                        CategoryId = categories.First(c => c.categoryName == "Kỹ năng sống").CategoryId,
                        ImageUrl = "~/images/phat-trien-ky-nang-lanh-dao_outline.JPG",
                        Rating = 4.9f,
                        RatingCount = 18
                    },
                    new Book
                    {
                        bookName = "7 Thói Quen Hiệu Quả",
                        description = "Bảy thói quen để thành công trong cuộc sống.",
                        author = "Stephen R. Covey",
                        bookStatus = true,
                        quantity = 15,
                        price = 115000,
                        CategoryId = categories.First(c => c.categoryName == "Kỹ năng sống").CategoryId,
                        ImageUrl = "~/images/product-item6.jpg",
                        Rating = 4.7f,
                        RatingCount = 14
                    },
                    new Book
                    {
                        bookName = "Nghệ Thuật Giao Tiếp",
                        description = "Kỹ năng giao tiếp hiệu quả trong mọi tình huống.",
                        author = "Dale Carnegie",
                        bookStatus = true,
                        quantity = 12,
                        price = 90000,
                        CategoryId = categories.First(c => c.categoryName == "Kỹ năng sống").CategoryId,
                        ImageUrl = "~/images/product-item7.jpg",
                        Rating = 4.8f,
                        RatingCount = 16
                    },
                    new Book
                    {
                        bookName = "Quản Lý Thời Gian Hiệu Quả",
                        description = "Phương pháp quản lý thời gian để tăng năng suất.",
                        author = "Brian Tracy",
                        bookStatus = true,
                        quantity = 14,
                        price = 85000,
                        CategoryId = categories.First(c => c.categoryName == "Kỹ năng sống").CategoryId,
                        ImageUrl = "~/images/product-item8.jpg",
                        Rating = 4.9f,
                        RatingCount = 22
                    },
                    // Công nghệ (4 sách)
                    new Book
                    {
                        bookName = "Lập Trình Python Cơ Bản",
                        description = "Hướng dẫn học lập trình Python cho người mới bắt đầu.",
                        author = "Nguyễn Văn A",
                        bookStatus = true,
                        quantity = 12,
                        price = 135000,
                        CategoryId = categories.First(c => c.categoryName == "Công nghệ").CategoryId,
                        ImageUrl = "~/images/product-item3.jpg",
                        Rating = 4.6f,
                        RatingCount = 15
                    },
                    new Book
                    {
                        bookName = "JavaScript Nâng Cao",
                        description = "Kỹ thuật lập trình JavaScript chuyên sâu.",
                        author = "David Flanagan",
                        bookStatus = true,
                        quantity = 10,
                        price = 145000,
                        CategoryId = categories.First(c => c.categoryName == "Công nghệ").CategoryId,
                        ImageUrl = "~/images/product-item4.jpg",
                        Rating = 4.7f,
                        RatingCount = 18
                    },
                    new Book
                    {
                        bookName = "Machine Learning Cơ Bản",
                        description = "Giới thiệu về machine learning và ứng dụng.",
                        author = "Andrew Ng",
                        bookStatus = true,
                        quantity = 8,
                        price = 180000,
                        CategoryId = categories.First(c => c.categoryName == "Công nghệ").CategoryId,
                        ImageUrl = "~/images/product-item5.jpg",
                        Rating = 4.8f,
                        RatingCount = 20
                    },
                    new Book
                    {
                        bookName = "Web Development Hiện Đại",
                        description = "Phát triển web với các công nghệ mới nhất.",
                        author = "Jon Duckett",
                        bookStatus = true,
                        quantity = 11,
                        price = 160000,
                        CategoryId = categories.First(c => c.categoryName == "Công nghệ").CategoryId,
                        ImageUrl = "~/images/product-item6.jpg",
                        Rating = 4.9f,
                        RatingCount = 25
                    },
                    // Lịch sử (4 sách)
                    new Book
                    {
                        bookName = "Lịch Sử Việt Nam",
                        description = "Khái quát lịch sử Việt Nam từ cổ đại đến hiện đại.",
                        author = "Trần Trọng Kim",
                        bookStatus = true,
                        quantity = 8,
                        price = 150000,
                        CategoryId = categories.First(c => c.categoryName == "Lịch sử").CategoryId,
                        ImageUrl = "~/images/product-item4.jpg",
                        Rating = 4.5f,
                        RatingCount = 10
                    },
                    new Book
                    {
                        bookName = "Lịch Sử Thế Giới",
                        description = "Tổng quan lịch sử thế giới qua các thời kỳ.",
                        author = "Will Durant",
                        bookStatus = true,
                        quantity = 6,
                        price = 200000,
                        CategoryId = categories.First(c => c.categoryName == "Lịch sử").CategoryId,
                        ImageUrl = "~/images/product-item7.jpg",
                        Rating = 4.8f,
                        RatingCount = 15
                    },
                    new Book
                    {
                        bookName = "Chiến Tranh Và Hòa Bình",
                        description = "Tác phẩm văn học lịch sử nổi tiếng.",
                        author = "Leo Tolstoy",
                        bookStatus = true,
                        quantity = 9,
                        price = 175000,
                        CategoryId = categories.First(c => c.categoryName == "Lịch sử").CategoryId,
                        ImageUrl = "~/images/product-item8.jpg",
                        Rating = 4.9f,
                        RatingCount = 20
                    },
                    new Book
                    {
                        bookName = "Lịch Sử Trung Quốc",
                        description = "Khám phá lịch sử văn minh Trung Hoa.",
                        author = "John Keay",
                        bookStatus = true,
                        quantity = 7,
                        price = 165000,
                        CategoryId = categories.First(c => c.categoryName == "Lịch sử").CategoryId,
                        ImageUrl = "~/images/product-item1.jpg",
                        Rating = 4.6f,
                        RatingCount = 12
                    },
                    // Tâm lý (4 sách)
                    new Book
                    {
                        bookName = "Tâm Lý Học Thành Công",
                        description = "Khám phá bí quyết thành công qua góc nhìn tâm lý học.",
                        author = "Carol S. Dweck",
                        bookStatus = true,
                        quantity = 14,
                        price = 99000,
                        CategoryId = categories.First(c => c.categoryName == "Tâm lý").CategoryId,
                        ImageUrl = "~/images/product-item5.jpg",
                        Rating = 4.7f,
                        RatingCount = 16
                    },
                    new Book
                    {
                        bookName = "Tâm Lý Học Đám Đông",
                        description = "Nghiên cứu về hành vi của đám đông.",
                        author = "Gustave Le Bon",
                        bookStatus = true,
                        quantity = 11,
                        price = 105000,
                        CategoryId = categories.First(c => c.categoryName == "Tâm lý").CategoryId,
                        ImageUrl = "~/images/product-item2.jpg",
                        Rating = 4.9f,
                        RatingCount = 25
                    },
                    new Book
                    {
                        bookName = "Nghệ Thuật Tư Duy Rành Mạch",
                        description = "Cải thiện kỹ năng tư duy và ra quyết định.",
                        author = "Rolf Dobelli",
                        bookStatus = true,
                        quantity = 13,
                        price = 95000,
                        CategoryId = categories.First(c => c.categoryName == "Tâm lý").CategoryId,
                        ImageUrl = "~/images/product-item3.jpg",
                        Rating = 4.8f,
                        RatingCount = 18
                    },
                    new Book
                    {
                        bookName = "Tâm Lý Học Hạnh Phúc",
                        description = "Con đường tìm kiếm hạnh phúc thực sự.",
                        author = "Martin Seligman",
                        bookStatus = true,
                        quantity = 12,
                        price = 115000,
                        CategoryId = categories.First(c => c.categoryName == "Tâm lý").CategoryId,
                        ImageUrl = "~/images/product-item4.jpg",
                        Rating = 4.7f,
                        RatingCount = 14
                    }
                    };

                context.Books.AddRange(books);
                context.SaveChanges();
            }
        }
    }
}


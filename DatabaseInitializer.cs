using Microsoft.EntityFrameworkCore;
using QLNhaSach1.Models;

namespace QLNhaSach1
{
    public class DatabaseInitializer
    {
        private readonly AppDbContext _context;

        public DatabaseInitializer(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAdminUserAsync()
        {
            // Kiểm tra xem có tài khoản admin nào trong cơ sở dữ liệu chưa
            bool hasAdmin = await _context.Users.AnyAsync(u => u.Role == Role.Admin);

            if (!hasAdmin)
            {
                // Tạo tài khoản admin mặc định
                var adminUser = new User
                {
                    UserName = "Administrator",
                    Email = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                    Phone = "1234567890", // Số điện thoại mặc định
                    Address = "Default Address",
                    Role = Role.Admin
                };

                _context.Users.Add(adminUser);
                await _context.SaveChangesAsync();
            }
        }
    }
}
using Microsoft.EntityFrameworkCore;
using QLNhaSach1.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Book> Books { get; set; }
    public DbSet<Category> Categories { get; set; }

    public DbSet<User> Users { get; set; }
}

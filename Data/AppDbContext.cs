using Microsoft.EntityFrameworkCore;
using QLNhaSach1.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Book> Books { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Quan hệ User - Cart (1-n)
        modelBuilder.Entity<Cart>()
            .HasKey(c => c.CartId);

        modelBuilder.Entity<Cart>()
            .HasOne(c => c.User)
            .WithMany(u => u.Carts)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Quan hệ Cart - CartItem (1-n)
        modelBuilder.Entity<CartItem>()
            .HasKey(ci => ci.CartItemId);

        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Cart)
            .WithMany(c => c.CartItems)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        // Quan hệ CartItem - Book (n-1)
        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Book)
            .WithMany()
            .HasForeignKey(ci => ci.BookId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

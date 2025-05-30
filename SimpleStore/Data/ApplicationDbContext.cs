using Microsoft.EntityFrameworkCore;
using SimpleStore.Models;
namespace SimpleStore.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure unique constraint for email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            // Seed some sample products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Laptop Pro X1",
                    Description = "High-performance laptop with 16GB RAM, 512GB SSD, and a powerful processor for all your computing needs.",
                    Price = 1299.99m,
                    ImageUrl = "/images/products/laptop-x1.jpg",
                    CreatedAt = DateTime.Now,
                    Stock = 15,
                    Category = "Electronics",
                    IsActive = true
                },
                new Product
                {
                    Id = 2,
                    Name = "Smartphone Galaxy S",
                    Description = "Latest smartphone with 6.5-inch AMOLED display, 128GB storage, and an impressive camera system.",
                    Price = 899.99m,
                    ImageUrl = "/images/products/galaxy-s.jpg",
                    CreatedAt = DateTime.Now,
                    Stock = 25,
                    Category = "Phones",
                    IsActive = true
                },
                new Product
                {
                    Id = 3,
                    Name = "Wireless Headphones",
                    Description = "Premium wireless headphones with noise cancellation and 30-hour battery life.",
                    Price = 249.99m,
                    ImageUrl = "/images/products/headphones.jpg",
                    CreatedAt = DateTime.Now,
                    Stock = 30,
                    Category = "Audio",
                    IsActive = true
                },
                new Product
                {
                    Id = 4,
                    Name = "Smart Watch Series 5",
                    Description = "Track your fitness, receive notifications, and more with this sleek smart watch.",
                    Price = 349.99m,
                    ImageUrl = "/images/products/smartwatch.jpg",
                    CreatedAt = DateTime.Now,
                    Stock = 20,
                    Category = "Wearables",
                    IsActive = true
                },
                new Product
                {
                    Id = 5,
                    Name = "Tablet Pro 12",
                    Description = "12-inch tablet with retina display, perfect for work and entertainment on the go.",
                    Price = 799.99m,
                    ImageUrl = "/images/products/tablet-pro.jpg",
                    CreatedAt = DateTime.Now,
                    Stock = 10,
                    Category = "Electronics",
                    IsActive = true
                }
            );
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);
        }
    }
}
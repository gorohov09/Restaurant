using Microsoft.EntityFrameworkCore;
using Restaurant.Services.ProductAPI.Models;

namespace Restaurant.Services.ProductAPI.DbContexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Product>().HasData(new Product
            {
                ProductId = 1,
                Name = "Самса с говядиной",
                Price = 15,
                Description = "Лучшая самса.",
                CategoryName = "Хлебо-булочные изделия"
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                ProductId = 2,
                Name = "Пирожок с луком",
                Price = 13.99,
                Description = "Поистине вкусный и хороший пиродок.",
                CategoryName = "Хлебо-булочные изделия"
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                ProductId = 3,
                Name = "Закрытая пица",
                Price = 10.99,
                Description = "Новая разновидность пицы на рынке",
                CategoryName = "Пицы"
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                ProductId = 4,
                Name = "Открытая пица",
                Price = 15,
                Description = "Классический вариант пицы.",
                CategoryName = "Пицы"
            });
        }
    }
}

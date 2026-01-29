using Microsoft.EntityFrameworkCore;
using ThirdStepTask.Product.Persistence.Context;

namespace ThirdStepTask.Product.Persistence.Seeders
{
    /// <summary>
    /// Database seeder for initial product data
    /// </summary>
    public static class ProductDbSeeder
    {
        public static async Task SeedAsync(ProductDbContext context)
        {
            // Ensure database is created
            await context.Database.MigrateAsync();

            // Seed Product Categories
            if (!await context.ProductCategories.AnyAsync())
            {
                var categories = new List<Domain.Entities.ProductCategory>
            {
                new Domain.Entities.ProductCategory
                {
                    Name = "Electronics",
                    Description = "Electronic devices and accessories",
                    Slug = "electronics",
                    DisplayOrder = 1,
                    IsActive = true
                },
                new Domain.Entities.ProductCategory
                {
                    Name = "Clothing",
                    Description = "Apparel and fashion items",
                    Slug = "clothing",
                    DisplayOrder = 2,
                    IsActive = true
                },
                new Domain.Entities.ProductCategory
                {
                    Name = "Books",
                    Description = "Books and publications",
                    Slug = "books",
                    DisplayOrder = 3,
                    IsActive = true
                },
                new Domain.Entities.ProductCategory
                {
                    Name = "Home & Garden",
                    Description = "Home and garden products",
                    Slug = "home-garden",
                    DisplayOrder = 4,
                    IsActive = true
                },
                new Domain.Entities.ProductCategory
                {
                    Name = "Sports",
                    Description = "Sports equipment and accessories",
                    Slug = "sports",
                    DisplayOrder = 5,
                    IsActive = true
                }
            };

                await context.ProductCategories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // Seed Products
            if (!await context.Products.AnyAsync())
            {
                var products = new List<Domain.Entities.Product>
            {
                new Domain.Entities.Product
                {
                    Name = "Laptop Dell XPS 15",
                    Description = "High-performance laptop with Intel i7 processor, 16GB RAM, 512GB SSD",
                    SKU = "DELL-XPS15-001",
                    Price = 1299.99m,
                    StockQuantity = 25,
                    Category = "Electronics",
                    IsActive = true
                },
                new Domain.Entities.Product
                {
                    Name = "iPhone 15 Pro",
                    Description = "Latest iPhone with A17 Pro chip, 256GB storage",
                    SKU = "APPLE-IP15P-256",
                    Price = 1199.99m,
                    StockQuantity = 50,
                    Category = "Electronics",
                    IsActive = true,
                    DiscountPercentage = 10,
                    DiscountValidUntil = DateTime.UtcNow.AddDays(30)
                },
                new Domain.Entities.Product
                {
                    Name = "Samsung Galaxy S24 Ultra",
                    Description = "Premium smartphone with S Pen, 512GB storage",
                    SKU = "SAMSUNG-S24U-512",
                    Price = 1299.99m,
                    StockQuantity = 30,
                    Category = "Electronics",
                    IsActive = true
                },
                new Domain.Entities.Product
                {
                    Name = "Men's Classic T-Shirt",
                    Description = "100% cotton comfortable t-shirt",
                    SKU = "CLOTH-TSHIRT-M-001",
                    Price = 29.99m,
                    StockQuantity = 100,
                    Category = "Clothing",
                    IsActive = true
                },
                new Domain.Entities.Product
                {
                    Name = "Women's Running Shoes",
                    Description = "Lightweight running shoes with excellent cushioning",
                    SKU = "SPORT-SHOES-W-001",
                    Price = 89.99m,
                    StockQuantity = 75,
                    Category = "Sports",
                    IsActive = true,
                    DiscountPercentage = 15,
                    DiscountValidUntil = DateTime.UtcNow.AddDays(15)
                },
                new Domain.Entities.Product
                {
                    Name = "The Art of Computer Programming",
                    Description = "Classic book by Donald Knuth",
                    SKU = "BOOK-TAOCP-001",
                    Price = 149.99m,
                    StockQuantity = 20,
                    Category = "Books",
                    IsActive = true
                },
                new Domain.Entities.Product
                {
                    Name = "Smart LED Bulb",
                    Description = "WiFi-enabled smart bulb with color changing",
                    SKU = "HOME-BULB-001",
                    Price = 19.99m,
                    StockQuantity = 200,
                    Category = "Home & Garden",
                    IsActive = true
                },
                new Domain.Entities.Product
                {
                    Name = "Yoga Mat Premium",
                    Description = "Non-slip yoga mat with carrying strap",
                    SKU = "SPORT-YOGA-001",
                    Price = 39.99m,
                    StockQuantity = 60,
                    Category = "Sports",
                    IsActive = true
                },
                new Domain.Entities.Product
                {
                    Name = "Wireless Mouse",
                    Description = "Ergonomic wireless mouse with long battery life",
                    SKU = "ELECT-MOUSE-001",
                    Price = 24.99m,
                    StockQuantity = 150,
                    Category = "Electronics",
                    IsActive = true
                },
                new Domain.Entities.Product
                {
                    Name = "Coffee Maker",
                    Description = "Programmable coffee maker with thermal carafe",
                    SKU = "HOME-COFFEE-001",
                    Price = 79.99m,
                    StockQuantity = 40,
                    Category = "Home & Garden",
                    IsActive = true,
                    DiscountPercentage = 20,
                    DiscountValidUntil = DateTime.UtcNow.AddDays(7)
                }
            };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }
        }
    }
}

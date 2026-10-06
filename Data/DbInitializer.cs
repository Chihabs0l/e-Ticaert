using ShoppingApp.Models;
using ShoppingApp.Helpers;

namespace ShoppingApp.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext db)
    {
        db.Database.EnsureCreated();

        // Seed admin account
        if (!db.Users.Any(u => u.Role == "Admin"))
        {
            db.Users.Add(new ApplicationUser
            {
                FullName = "Administrator",
                Email = "admin@shop.com",
                PasswordHash = PasswordHelper.Hash("Admin123!"),
                Role = "Admin"
            });
        }

        // Seed client demo account
        if (!db.Users.Any(u => u.Email == "client@shop.com"))
        {
            db.Users.Add(new ApplicationUser
            {
                FullName = "Demo Client",
                Email = "client@shop.com",
                PasswordHash = PasswordHelper.Hash("Client123!"),
                Role = "Client"
            });
        }

        // Seed sample products
        if (!db.Products.Any())
        {
            db.Products.AddRange(
                new Product { Name = "Wireless Headphones", Description = "Noise cancelling, 30h battery life.", Price = 79.99m, Stock = 25, ImageUrl = @"./wwwroot/Images/Wireless Headphones.avif" },
                new Product { Name = "Mechanical Keyboard", Description = "RGB backlit, hot-swappable switches.", Price = 129.50m, Stock = 15, ImageUrl = "https://placehold.co/600x400?text=Keyboard" },
                new Product { Name = "Smart Watch", Description = "Heart-rate monitor, GPS, 7-day battery.", Price = 199.00m, Stock = 30, ImageUrl = "https://placehold.co/600x400?text=Smart+Watch" },
                new Product { Name = "USB-C Hub", Description = "7-in-1 hub with HDMI and PD charging.", Price = 39.99m, Stock = 50, ImageUrl = "https://placehold.co/600x400?text=USB-C+Hub" },
                new Product { Name = "Webcam 1080p", Description = "Full HD webcam with stereo mic.", Price = 59.00m, Stock = 40, ImageUrl = "https://placehold.co/600x400?text=Webcam" }
            );
        }

        db.SaveChanges();
    }
}

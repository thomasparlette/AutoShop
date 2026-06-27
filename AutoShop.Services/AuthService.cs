using AutoShop.Core.Entities;
using AutoShop.Core.Enums;
using AutoShop.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AutoShop.Services;

public class AuthService
{
    private readonly TechnicianService _technicianService = new();
    public void EnsureDefaults()
    {
        using var db = CreateContext();

        db.Database.Migrate();

        if (!db.ShopSettings.Any())
        {
            db.ShopSettings.Add(new ShopSettings
            {
                Id = 1,
                ShopName = "AutoShop",
                TaxRate = 0m,
                InvoicePrefix = "WO-",
                NextInvoiceNumber = 1,
                ReceiptFooterText = "Thank you for your business.",
                DefaultLaborRate = 0m,
                PartMarkupPercent = 35m
            });
        }

        if (!db.Users.Any())
        {
            db.Users.Add(new AppUser
            {
                UserName = "admin",
                DisplayName = "Administrator",
                Role = UserRole.Admin | UserRole.Technician | UserRole.Finance,
                IsActive = true,
                CreatedAt = DateTime.Now,
                PasswordHash = PasswordHasher.HashPassword("Admin123!")
            });
        }

        db.SaveChanges();
    }

    public AppUser? Authenticate(string userName, string password)
    {
        using var db = CreateContext();

        var user = db.Users.FirstOrDefault(u =>
            u.UserName.ToLower() == userName.Trim().ToLower() &&
            u.IsActive);

        if (user == null)
            return null;

        if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
            return null;

        user.LastLoginAt = DateTime.Now;
        db.SaveChanges();

        return user;
    }

    public ShopSettings GetShopSettings()
    {
        using var db = CreateContext();

        return db.ShopSettings.First();
    }

    public void SaveShopSettings(ShopSettings settings)
    {
        using var db = CreateContext();

        var existing = db.ShopSettings.FirstOrDefault(s => s.Id == settings.Id);
        if (existing == null)
        {
            db.ShopSettings.Add(settings);
        }
        else
        {
            db.Entry(existing).CurrentValues.SetValues(settings);
        }

        db.SaveChanges();
    }


    private static AppDbContext CreateContext()
    {
        var factory = new AppDbContextFactory();
        return factory.CreateDbContext(Array.Empty<string>());
    }
}
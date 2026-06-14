using AutoShop.Core.Entities;
using AutoShop.Core.Enums;
using AutoShop.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AutoShop.Services;

public class AuthService
{
    public void EnsureDefaults()
    {
        using var db = CreateContext();

        if (!db.ShopSettings.Any())
        {
            db.ShopSettings.Add(new ShopSettings
            {
                Id = 1,
                ShopName = "AutoShop",
                TaxRate = 0m,
                InvoicePrefix = "WO-",
                NextInvoiceNumber = 1,
                ReceiptFooterText = "Thank you for your business."
            });
        }

        if (!db.Users.Any())
        {
            var admin = new AppUser
            {
                UserName = "admin",
                DisplayName = "Administrator",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.Now,
                PasswordHash = HashPassword("Admin123!")
            };

            db.Users.Add(admin);
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

        if (!VerifyPassword(password, user.PasswordHash))
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

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var pbkdf2 = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            100_000,
            HashAlgorithmName.SHA256,
            32);

        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(pbkdf2)}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2)
            return false;

        var salt = Convert.FromBase64String(parts[0]);
        var expectedHash = Convert.FromBase64String(parts[1]);

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            100_000,
            HashAlgorithmName.SHA256,
            32);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static AppDbContext CreateContext()
    {
        var factory = new AppDbContextFactory();
        return factory.CreateDbContext(Array.Empty<string>());
    }
}
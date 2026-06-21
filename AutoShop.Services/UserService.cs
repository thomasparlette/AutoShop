using AutoShop.Core.Entities;
using AutoShop.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoShop.Services;

public class UserService
{
    public List<AppUser> GetUsers(bool includeInactive = false, string? searchText = null)
    {
        using var db = CreateContext();

        var query = db.Users.AsNoTracking().AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(u => u.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            searchText = searchText.Trim();

            query = query.Where(u =>
                EF.Functions.Like(u.UserName, $"%{searchText}%") ||
                EF.Functions.Like(u.DisplayName, $"%{searchText}%"));
        }

        return query
            .OrderBy(u => u.DisplayName)
            .ThenBy(u => u.UserName)
            .ToList();
    }

    public AppUser? GetUserById(int id)
    {
        using var db = CreateContext();
        return db.Users.FirstOrDefault(u => u.Id == id);
    }

    public AppUser SaveUser(AppUser user, string? newPassword = null)
    {
        using var db = CreateContext();

        AppUser entity;

        if (user.Id == 0)
        {
            entity = new AppUser
            {
                CreatedAt = DateTime.Now
            };
            db.Users.Add(entity);
        }
        else
        {
            entity = db.Users.FirstOrDefault(u => u.Id == user.Id)
                     ?? throw new InvalidOperationException("User not found.");
        }

        entity.UserName = user.UserName.Trim();
        entity.DisplayName = user.DisplayName.Trim();
        entity.Role = user.Role;
        entity.IsActive = user.IsActive;

        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            entity.PasswordHash = PasswordHasher.HashPassword(newPassword);
        }

        db.SaveChanges();
        return entity;
    }

    public void DeactivateUser(int id)
    {
        using var db = CreateContext();

        var user = db.Users.FirstOrDefault(u => u.Id == id);
        if (user == null)
            return;

        user.IsActive = false;
        db.SaveChanges();
    }

    private static AppDbContext CreateContext()
    {
        var factory = new AppDbContextFactory();
        return factory.CreateDbContext(Array.Empty<string>());
    }
}
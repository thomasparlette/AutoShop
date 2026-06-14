using AutoShop.Core.Enums;

namespace AutoShop.Core.Entities;

public class AppUser
{
    public int Id { get; set; }

    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Standard;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? LastLoginAt { get; set; }
}
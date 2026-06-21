using AutoShop.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AutoShop.Core.Entities;

public class AppUser
{
    public int Id { get; set; }

    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.None;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? LastLoginAt { get; set; }

    [NotMapped]
    public string RoleDisplay =>
        Role == UserRole.None
            ? "Standard"
            : string.Join(", ",
                Enum.GetValues<UserRole>()
                    .Where(r => r != UserRole.None && Role.HasFlag(r)));
}
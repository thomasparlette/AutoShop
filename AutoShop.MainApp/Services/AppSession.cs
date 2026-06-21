using AutoShop.Core.Entities;
using AutoShop.Core.Enums;

namespace AutoShop.MainApp.Services;

public static class AppSession
{
    public static AppUser? CurrentUser { get; set; }

    public static bool HasRole(UserRole role) =>
        CurrentUser?.Role.HasFlag(role) == true;

    public static bool IsAdmin => HasRole(UserRole.Admin);
}
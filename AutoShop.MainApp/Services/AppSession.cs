using AutoShop.Core.Entities;

namespace AutoShop.MainApp.Services;

public static class AppSession
{
    public static AppUser? CurrentUser { get; set; }

    public static bool IsAdmin =>
        CurrentUser?.Role == AutoShop.Core.Enums.UserRole.Admin;
}
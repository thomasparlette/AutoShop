using System;

namespace AutoShop.Core.Enums;

[Flags]
public enum UserRole
{
    None = 0,
    Technician = 1,
    Finance = 2,
    Admin = 4
}
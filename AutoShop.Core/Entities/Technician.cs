namespace AutoShop.Core.Entities;

public class Technician
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }

    public bool Active { get; set; } = true;

    public decimal LaborRate { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
    public string UserName { get; set; } = string.Empty;
}
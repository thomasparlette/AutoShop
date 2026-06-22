namespace AutoShop.Core.Entities;

public class ShopSettings
{
    public int Id { get; set; }

    public string ShopName { get; set; } = "AutoShop";
    public string? LogoPath { get; set; }

    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }

    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }

    public string? TaxId { get; set; }
    public decimal TaxRate { get; set; }
    public decimal PartMarkupPercent { get; set; } = 35m;

    public string? ReceiptFooterText { get; set; }
    public string? InvoicePrefix { get; set; }
    public int NextInvoiceNumber { get; set; } = 1;

    public string? BusinessHours { get; set; }
    public string? DefaultThankYouMessage { get; set; }
    public decimal DefaultLaborRate { get; set; } = 0m;
}
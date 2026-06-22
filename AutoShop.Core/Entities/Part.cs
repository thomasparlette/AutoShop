namespace AutoShop.Core.Entities;

public class Part
{
    public int Id { get; set; }

    public string PartNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public decimal Cost { get; set; }
    public decimal SellPrice { get; set; }

    public int QuantityOnHand { get; set; }
    public int ReorderLevel { get; set; }

    public string? Supplier { get; set; }
    public bool Active { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}
namespace AutoShop.Core.Entities;

public class PurchaseOrderLineItem
{
    public int Id { get; set; }

    public int PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }

    public int? PartId { get; set; }
    public Part? Part { get; set; }

    public string PartNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int QuantityOrdered { get; set; }
    public decimal UnitCost { get; set; }
}
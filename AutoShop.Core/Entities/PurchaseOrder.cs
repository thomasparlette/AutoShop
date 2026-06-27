using AutoShop.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoShop.Core.Entities;

public class PurchaseOrder
{
    public int Id { get; set; }

    public string PoNumber { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;

    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? OrderedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }

    public string? Notes { get; set; }

    public ICollection<PurchaseOrderLineItem> LineItems { get; set; } = new List<PurchaseOrderLineItem>();

    [NotMapped]
    public int TotalOrderedQuantity => LineItems.Sum(x => x.QuantityOrdered);

    [NotMapped]
    public int TotalReceivedQuantity => LineItems.Sum(x => x.QuantityReceived);

    [NotMapped]
    public decimal OrderedTotal => LineItems.Sum(x => x.QuantityOrdered * x.UnitCost);

    [NotMapped]
    public decimal ReceivedTotal => LineItems.Sum(x => x.QuantityReceived * x.UnitCost);
}
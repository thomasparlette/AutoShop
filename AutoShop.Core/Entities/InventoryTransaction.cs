using System;

namespace AutoShop.Core.Entities;

public class InventoryTransaction
{
    public int Id { get; set; }

    public int PartId { get; set; }
    public Part? Part { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.Now;

    public int QuantityChange { get; set; }
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }

    public string TransactionType { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
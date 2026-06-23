using AutoShop.Core.Entities;
using AutoShop.Core.Enums;
using AutoShop.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoShop.Services;

public class PurchaseOrderService
{
    public PurchaseOrder CreateDraftFromLowStock(string supplier)
    {
        using var db = CreateContext();

        var lowStockParts = db.Parts
            .Where(p => p.Active && p.ReorderLevel > 0 && p.QuantityOnHand <= p.ReorderLevel)
            .ToList();

        var po = new PurchaseOrder
        {
            PoNumber = $"PO-{DateTime.Now:yyyyMMdd-HHmmss}",
            Supplier = supplier,
            Status = PurchaseOrderStatus.Draft,
            CreatedAt = DateTime.Now
        };

        foreach (var part in lowStockParts)
        {
            var qtyToOrder = Math.Max(part.ReorderLevel - part.QuantityOnHand, 1);

            po.LineItems.Add(new PurchaseOrderLineItem
            {
                PartId = part.Id,
                PartNumber = part.PartNumber,
                Description = part.Description,
                QuantityOrdered = qtyToOrder,
                UnitCost = part.Cost
            });
        }

        db.PurchaseOrders.Add(po);
        db.SaveChanges();
        return po;
    }

    public List<PurchaseOrder> GetOpenPurchaseOrders()
    {
        using var db = CreateContext();

        return db.PurchaseOrders
            .Include(p => p.LineItems)
            .Where(p => p.Status == PurchaseOrderStatus.Draft || p.Status == PurchaseOrderStatus.Ordered || p.Status == PurchaseOrderStatus.PartialReceived)
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToList();
    }

    private static AppDbContext CreateContext()
    {
        var factory = new AppDbContextFactory();
        return factory.CreateDbContext(Array.Empty<string>());
    }
}
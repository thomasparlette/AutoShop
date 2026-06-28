using AutoShop.Core.Entities;
using AutoShop.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoShop.Services;

public class InventoryService
{
    public List<Part> GetLowStockParts()
    {
        using var db = CreateContext();

        return db.Parts
            .AsNoTracking()
            .Where(p => p.Active && p.ReorderLevel > 0 && p.QuantityOnHand < p.ReorderLevel)
            .OrderBy(p => p.PartNumber)
            .ToList();
    }

    public List<InventoryTransaction> GetTransactionsForPart(int partId)
    {
        using var db = CreateContext();

        return db.InventoryTransactions
            .Include(t => t.Part)
            .AsNoTracking()
            .Where(t => t.PartId == partId)
            .OrderByDescending(t => t.TransactionDate)
            .ToList();
    }

    public void AdjustQuantity(
        int partId,
        int delta,
        string transactionType = "Adjustment",
        string referenceNumber = "",
        string notes = "")
    {
        using var db = CreateContext();

        var part = db.Parts.FirstOrDefault(p => p.Id == partId);
        if (part == null)
            return;

        ApplyAdjustment(
            db,
            part,
            delta,
            transactionType,
            referenceNumber,
            notes);

        db.SaveChanges();
    }

    private static void ApplyAdjustment(
        AppDbContext db,
        Part part,
        int delta,
        string transactionType,
        string referenceNumber,
        string notes)
    {
        var beforeQty = part.QuantityOnHand;
        part.QuantityOnHand += delta;
        part.UpdatedAt = DateTime.Now;

        db.InventoryTransactions.Add(new InventoryTransaction
        {
            PartId = part.Id,
            TransactionDate = DateTime.Now,
            QuantityBefore = beforeQty,
            QuantityAfter = part.QuantityOnHand,
            QuantityChange = delta,
            TransactionType = transactionType ?? string.Empty,
            ReferenceNumber = referenceNumber ?? string.Empty,
            Notes = notes ?? string.Empty
        });
    }

    private static AppDbContext CreateContext()
    {
        var factory = new AppDbContextFactory();
        return factory.CreateDbContext(Array.Empty<string>());
    }
}
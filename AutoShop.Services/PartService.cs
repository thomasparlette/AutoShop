using AutoShop.Core.Entities;
using AutoShop.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoShop.Services;

public class PartService
{
    public List<Part> GetParts(bool includeInactive = false, string? searchText = null)
    {
        using var db = CreateContext();

        var query = db.Parts.AsNoTracking().AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(p => p.Active);
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            searchText = searchText.Trim();

            query = query.Where(p =>
                EF.Functions.Like(p.PartNumber, $"%{searchText}%") ||
                EF.Functions.Like(p.Description, $"%{searchText}%") ||
                (p.Supplier != null && EF.Functions.Like(p.Supplier, $"%{searchText}%")));
        }

        return query
            .OrderBy(p => p.PartNumber)
            .ThenBy(p => p.Description)
            .ToList();
    }

    public Part? GetPartById(int id)
    {
        using var db = CreateContext();
        return db.Parts.FirstOrDefault(p => p.Id == id);
    }

    public Part? GetPartByNumber(string partNumber)
    {
        using var db = CreateContext();
        return db.Parts.FirstOrDefault(p => p.PartNumber == partNumber);
    }

    public Part SavePart(Part part)
    {
        using var db = CreateContext();

        if (part.Id == 0)
        {
            part.CreatedAt = DateTime.Now;
            db.Parts.Add(part);
        }
        else
        {
            part.UpdatedAt = DateTime.Now;
            db.Parts.Update(part);
        }

        db.SaveChanges();
        return part;
    }

    public void DeletePart(int id)
    {
        using var db = CreateContext();

        var part = db.Parts.FirstOrDefault(p => p.Id == id);
        if (part == null)
            return;

        var isReferenced =
            db.InventoryTransactions.Any(t => t.PartId == id) ||
            db.WorkOrderLineItems.Any(li => li.PartId == id) ||
            db.PurchaseOrderLineItems.Any(li => li.PartId == id);

        if (isReferenced)
        {
            part.Active = false;
            part.UpdatedAt = DateTime.Now;
            db.SaveChanges();
            return;
        }

        db.Parts.Remove(part);
        db.SaveChanges();
    }

    private static AppDbContext CreateContext()
    {
        var factory = new AppDbContextFactory();
        return factory.CreateDbContext(Array.Empty<string>());
    }
}
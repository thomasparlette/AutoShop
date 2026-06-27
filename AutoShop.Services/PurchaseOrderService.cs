using AutoShop.Core.Entities;
using AutoShop.Core.Enums;
using AutoShop.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoShop.Services;

public class PurchaseOrderService
{
    public List<PurchaseOrder> GetPurchaseOrders(string? searchText = null)
    {
        using var db = CreateContext();

        var query = db.PurchaseOrders
            .Include(p => p.LineItems)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            searchText = searchText.Trim();

            query = query.Where(p =>
                EF.Functions.Like(p.PoNumber, $"%{searchText}%") ||
                EF.Functions.Like(p.Supplier, $"%{searchText}%") ||
                (p.Notes != null && EF.Functions.Like(p.Notes, $"%{searchText}%")));
        }

        return query
            .OrderByDescending(p => p.CreatedAt)
            .ToList();
    }

    public PurchaseOrder? GetPurchaseOrderById(int id)
    {
        using var db = CreateContext();

        return db.PurchaseOrders
            .Include(p => p.LineItems)
            .ThenInclude(li => li.Part)
            .FirstOrDefault(p => p.Id == id);
    }

    public PurchaseOrder? CreateDraftFromLowStock(string? supplier = null)
    {
        using var db = CreateContext();

        var query = db.Parts
            .Where(p => p.Active && p.ReorderLevel > 0 && p.QuantityOnHand < p.ReorderLevel);

        if (!string.IsNullOrWhiteSpace(supplier))
        {
            query = query.Where(p => p.Supplier != null && p.Supplier == supplier);
        }

        var lowStockParts = query
            .OrderBy(p => p.PartNumber)
            .ToList();

        if (lowStockParts.Count == 0)
            return null;

        var po = new PurchaseOrder
        {
            PoNumber = GeneratePoNumber(),
            Supplier = string.IsNullOrWhiteSpace(supplier) ? "Multiple Suppliers" : supplier.Trim(),
            Status = PurchaseOrderStatus.Draft,
            CreatedAt = DateTime.Now,
            Notes = "Auto-generated from low inventory."
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
                QuantityReceived = 0,
                UnitCost = part.Cost
            });
        }

        db.PurchaseOrders.Add(po);
        db.SaveChanges();

        return GetPurchaseOrderById(po.Id);
    }

    public PurchaseOrder SavePurchaseOrder(PurchaseOrder purchaseOrder)
    {
        using var db = CreateContext();

        if (purchaseOrder.Id == 0)
        {
            if (string.IsNullOrWhiteSpace(purchaseOrder.PoNumber))
                purchaseOrder.PoNumber = GeneratePoNumber();

            if (purchaseOrder.CreatedAt == default)
                purchaseOrder.CreatedAt = DateTime.Now;

            NormalizeHeader(purchaseOrder);
            NormalizeLineItems(purchaseOrder);

            db.PurchaseOrders.Add(purchaseOrder);
            db.SaveChanges();

            return GetPurchaseOrderById(purchaseOrder.Id) ?? purchaseOrder;
        }

        var tracked = db.PurchaseOrders
            .Include(p => p.LineItems)
            .FirstOrDefault(p => p.Id == purchaseOrder.Id);

        if (tracked == null)
            throw new InvalidOperationException("Purchase order not found.");

        tracked.PoNumber = string.IsNullOrWhiteSpace(purchaseOrder.PoNumber)
            ? tracked.PoNumber
            : purchaseOrder.PoNumber.Trim();

        tracked.Supplier = purchaseOrder.Supplier?.Trim() ?? string.Empty;
        tracked.Notes = purchaseOrder.Notes;
        tracked.Status = purchaseOrder.Status;

        SyncLineItems(db, tracked, purchaseOrder.LineItems, adjustInventory: false);
        ApplyDates(tracked);

        db.SaveChanges();
        return GetPurchaseOrderById(tracked.Id) ?? tracked;
    }

    public PurchaseOrder ReceivePurchaseOrder(PurchaseOrder purchaseOrder)
    {
        using var db = CreateContext();

        var tracked = db.PurchaseOrders
            .Include(p => p.LineItems)
            .FirstOrDefault(p => p.Id == purchaseOrder.Id);

        if (tracked == null)
            throw new InvalidOperationException("Purchase order not found.");

        tracked.PoNumber = string.IsNullOrWhiteSpace(purchaseOrder.PoNumber)
            ? tracked.PoNumber
            : purchaseOrder.PoNumber.Trim();

        tracked.Supplier = purchaseOrder.Supplier?.Trim() ?? string.Empty;
        tracked.Notes = purchaseOrder.Notes;

        SyncLineItems(db, tracked, purchaseOrder.LineItems, adjustInventory: true);

        tracked.Status = DetermineStatus(tracked);

        if (tracked.Status is PurchaseOrderStatus.Ordered or PurchaseOrderStatus.PartialReceived or PurchaseOrderStatus.Received)
        {
            tracked.OrderedAt ??= DateTime.Now;
        }

        if (tracked.Status == PurchaseOrderStatus.Received)
        {
            tracked.ReceivedAt ??= DateTime.Now;
        }

        db.SaveChanges();
        return GetPurchaseOrderById(tracked.Id) ?? tracked;
    }

    public void MarkOrdered(int id)
    {
        using var db = CreateContext();

        var po = db.PurchaseOrders.FirstOrDefault(p => p.Id == id);
        if (po == null)
            return;

        po.Status = PurchaseOrderStatus.Ordered;
        po.OrderedAt ??= DateTime.Now;

        db.SaveChanges();
    }

    public void CancelPurchaseOrder(int id)
    {
        using var db = CreateContext();

        var po = db.PurchaseOrders.FirstOrDefault(p => p.Id == id);
        if (po == null)
            return;

        po.Status = PurchaseOrderStatus.Cancelled;
        db.SaveChanges();
    }

    private static void SyncLineItems(AppDbContext db,PurchaseOrder tracked,IEnumerable<PurchaseOrderLineItem> incomingLineItems,bool adjustInventory)
    {
        var incomingList = incomingLineItems.Select(CloneLineItem).ToList();
        var existingById = tracked.LineItems
            .Where(x => x.Id != 0)
            .ToDictionary(x => x.Id);

        var incomingIds = new HashSet<int>();

        foreach (var incoming in incomingList)
        {
            if (incoming.Id != 0 && existingById.TryGetValue(incoming.Id, out var existing))
            {
                if (adjustInventory)
                {
                    ApplyInventoryDelta(
                        db,
                        existing.PartId,
                        incoming.QuantityReceived - existing.QuantityReceived,
                        "Purchase Order",
                        tracked.PoNumber,
                        "Receiving purchase order");
                }

                existing.PartId = incoming.PartId;
                existing.PartNumber = incoming.PartNumber;
                existing.Description = incoming.Description;
                existing.QuantityOrdered = incoming.QuantityOrdered;
                existing.QuantityReceived = incoming.QuantityReceived;
                existing.UnitCost = incoming.UnitCost;

                incomingIds.Add(existing.Id);
            }
            else
            {
                if (adjustInventory)
                {
                    ApplyInventoryDelta(
                        db,
                        incoming.PartId,
                        incoming.QuantityReceived,
                        "Purchase Order",
                        tracked.PoNumber,
                        "Receiving purchase order");
                }

                incoming.Id = 0;
                tracked.LineItems.Add(incoming);
            }
        }

        var toRemove = tracked.LineItems
            .Where(x => x.Id != 0 && !incomingIds.Contains(x.Id))
            .ToList();

        foreach (var remove in toRemove)
        {
            if (adjustInventory)
            {
                ApplyInventoryDelta(
                    db,
                    remove.PartId,
                    -remove.QuantityReceived,
                    "Purchase Order",
                    tracked.PoNumber,
                    "Removing PO line item");
            }

            tracked.LineItems.Remove(remove);
            db.Remove(remove);
        }
    }

    private static void ApplyInventoryDelta(AppDbContext db,int? partId,int delta,string transactionType = "Adjustment",string referenceNumber = "",string notes = "")
    {
        if (!partId.HasValue || delta == 0)
            return;

        var part = db.Parts.FirstOrDefault(p => p.Id == partId.Value);
        if (part == null)
            return;

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
            TransactionType = transactionType,
            ReferenceNumber = referenceNumber ?? string.Empty,
            Notes = notes ?? string.Empty
        });
    }

    private static void NormalizeHeader(PurchaseOrder purchaseOrder)
    {
        purchaseOrder.PoNumber = string.IsNullOrWhiteSpace(purchaseOrder.PoNumber)
            ? GeneratePoNumber()
            : purchaseOrder.PoNumber.Trim();

        purchaseOrder.Supplier = purchaseOrder.Supplier?.Trim() ?? string.Empty;
        purchaseOrder.Notes = purchaseOrder.Notes?.Trim();
    }

    private static void NormalizeLineItems(PurchaseOrder purchaseOrder)
    {
        foreach (var item in purchaseOrder.LineItems)
        {
            item.PartNumber = item.PartNumber?.Trim() ?? string.Empty;
            item.Description = item.Description?.Trim() ?? string.Empty;
            if (item.QuantityOrdered < 0) item.QuantityOrdered = 0;
            if (item.QuantityReceived < 0) item.QuantityReceived = 0;
            if (item.UnitCost < 0) item.UnitCost = 0;
        }
    }

    private static void ApplyDates(PurchaseOrder purchaseOrder)
    {
        if (purchaseOrder.Status is PurchaseOrderStatus.Ordered or PurchaseOrderStatus.PartialReceived or PurchaseOrderStatus.Received)
        {
            purchaseOrder.OrderedAt ??= DateTime.Now;
        }

        if (purchaseOrder.Status == PurchaseOrderStatus.Received)
        {
            purchaseOrder.ReceivedAt ??= DateTime.Now;
        }
    }

    private static PurchaseOrderStatus DetermineStatus(PurchaseOrder purchaseOrder)
    {
        var ordered = purchaseOrder.LineItems.Sum(x => x.QuantityOrdered);
        var received = purchaseOrder.LineItems.Sum(x => x.QuantityReceived);

        if (received <= 0)
            return PurchaseOrderStatus.Ordered;

        if (received < ordered)
            return PurchaseOrderStatus.PartialReceived;

        return PurchaseOrderStatus.Received;
    }

    private static PurchaseOrderLineItem CloneLineItem(PurchaseOrderLineItem item)
    {
        return new PurchaseOrderLineItem
        {
            Id = item.Id,
            PurchaseOrderId = item.PurchaseOrderId,
            PartId = item.PartId,
            PartNumber = item.PartNumber,
            Description = item.Description,
            QuantityOrdered = item.QuantityOrdered,
            QuantityReceived = item.QuantityReceived,
            UnitCost = item.UnitCost
        };
    }

    private static string GeneratePoNumber()
    {
        return $"PO-{DateTime.Now:yyyyMMdd-HHmmss}";
    }

    private static AppDbContext CreateContext()
    {
        var factory = new AppDbContextFactory();
        return factory.CreateDbContext(Array.Empty<string>());
    }
}
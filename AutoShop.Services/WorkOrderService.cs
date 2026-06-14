using AutoShop.Core.Entities;
using AutoShop.Core.Enums;
using AutoShop.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoShop.Services;

public class WorkOrderService
{
    public List<Customer> GetCustomers()
    {
        using var db = CreateContext();

        return db.Customers
            .AsNoTracking()
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToList();
    }

    public List<Vehicle> GetVehicles()
    {
        using var db = CreateContext();

        return db.Vehicles
            .Include(v => v.Customer)
            .AsNoTracking()
            .OrderByDescending(v => v.Year)
            .ThenBy(v => v.Make)
            .ThenBy(v => v.Model)
            .ToList();
    }

    public List<WorkOrder> GetWorkOrders(string? searchText = null)
    {
        using var db = CreateContext();

        var query = db.WorkOrders
            .Include(w => w.Customer)
            .Include(w => w.Vehicle)
            .Include(w => w.LineItems)
            .Include(w => w.Payments)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            searchText = searchText.Trim();

            query = query.Where(w =>
                EF.Functions.Like(w.WorkOrderNumber, $"%{searchText}%") ||
                (w.Customer != null && EF.Functions.Like(w.Customer.FirstName, $"%{searchText}%")) ||
                (w.Customer != null && EF.Functions.Like(w.Customer.LastName, $"%{searchText}%")) ||
                (w.Vehicle != null && EF.Functions.Like(w.Vehicle.Make, $"%{searchText}%")) ||
                (w.Vehicle != null && EF.Functions.Like(w.Vehicle.Model, $"%{searchText}%")));
        }

        return query
            .OrderByDescending(w => w.CreatedAt)
            .ToList();
    }

    public WorkOrder SaveWorkOrder(WorkOrder workOrder)
    {
        using var db = CreateContext();

        RecalculateTotals(workOrder);

        if (string.IsNullOrWhiteSpace(workOrder.WorkOrderNumber))
        {
            workOrder.WorkOrderNumber = GenerateWorkOrderNumber();
        }

        if (workOrder.Id == 0)
        {
            db.WorkOrders.Add(workOrder);
        }
        else
        {
            db.WorkOrders.Update(workOrder);
        }

        db.SaveChanges();
        return workOrder;
    }

    public void DeleteWorkOrder(int id)
    {
        using var db = CreateContext();

        var workOrder = db.WorkOrders.FirstOrDefault(w => w.Id == id);
        if (workOrder == null)
            return;

        db.WorkOrders.Remove(workOrder);
        db.SaveChanges();
    }

    private static void RecalculateTotals(WorkOrder workOrder)
    {
        foreach (var item in workOrder.LineItems)
        {
            if (item.IsPart)
            {
                item.LaborHours = 0;
                item.LaborRate = 0;
                item.LineTotal = item.PartsCost;
            }
            else
            {
                item.PartsCost = 0;
                item.LineTotal = item.LaborHours * item.LaborRate;
            }
        }

        workOrder.LaborTotal = workOrder.LineItems
            .Where(x => !x.IsPart)
            .Sum(x => x.LineTotal);

        workOrder.PartsTotal = workOrder.LineItems
            .Where(x => x.IsPart)
            .Sum(x => x.LineTotal);

        workOrder.GrandTotal = workOrder.LaborTotal + workOrder.PartsTotal - workOrder.DiscountTotal + workOrder.TaxTotal;
        workOrder.BalanceDue = workOrder.GrandTotal - workOrder.AmountPaid;
    }

    private static string GenerateWorkOrderNumber()
    {
        return $"WO-{DateTime.Now:yyyyMMdd-HHmmss}";
    }

    private static AppDbContext CreateContext()
    {
        var factory = new AppDbContextFactory();
        return factory.CreateDbContext(Array.Empty<string>());
    }
    public WorkOrder? GetWorkOrderById(int id)
    {
        using var db = CreateContext();

        return db.WorkOrders
            .Include(w => w.Customer)
            .Include(w => w.Vehicle)
            .Include(w => w.LineItems)
            .Include(w => w.Payments)
            .FirstOrDefault(w => w.Id == id);
    }
}
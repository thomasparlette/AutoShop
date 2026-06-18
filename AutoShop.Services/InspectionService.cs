using AutoShop.Core.Entities;
using AutoShop.Core.Enums;
using AutoShop.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoShop.Services;

public class InspectionService
{
    public WorkOrderInspection GetOrCreateInspection(int workOrderId)
    {
        using var db = CreateContext();

        var inspection = db.WorkOrderInspections
            .Include(i => i.Items)
            .FirstOrDefault(i => i.WorkOrderId == workOrderId);

        if (inspection != null)
            return inspection;

        inspection = new WorkOrderInspection
        {
            WorkOrderId = workOrderId,
            CreatedAt = DateTime.Now,
            Items = BuildDefaultItems()
        };

        db.WorkOrderInspections.Add(inspection);
        db.SaveChanges();

        return db.WorkOrderInspections
            .Include(i => i.Items)
            .First(i => i.WorkOrderId == workOrderId);
    }

    public void SaveInspection(WorkOrderInspection inspection)
    {
        using var db = CreateContext();

        inspection.UpdatedAt = DateTime.Now;

        if (inspection.Id == 0)
        {
            db.WorkOrderInspections.Add(inspection);
        }
        else
        {
            db.WorkOrderInspections.Update(inspection);
        }

        db.SaveChanges();
    }

    private static List<WorkOrderInspectionItem> BuildDefaultItems()
    {
        var items = new List<WorkOrderInspectionItem>();

        void Add(string section, string name, int sortOrder)
        {
            items.Add(new WorkOrderInspectionItem
            {
                Section = section,
                ItemName = name,
                SortOrder = sortOrder,
                Status = InspectionStatus.NotInspected
            });
        }

        Add("Exterior", "Headlights / Taillights / Brake Lights / Turn Signals", 1);
        Add("Exterior", "Interior Lights", 2);
        Add("Exterior", "Windshield Washer / Wiper Operation / Wiper Blades", 3);
        Add("Exterior", "Parking Brake", 4);
        Add("Exterior", "Horn Operation", 5);
        Add("Exterior", "Clutch Operation (if applicable)", 6);
        Add("Exterior", "Cabin Air Filter", 7);

        Add("Under Hood", "Fluid Levels", 10);
        Add("Under Hood", "Air Filter Condition", 11);
        Add("Under Hood", "Belts and Radiator Hoses", 12);
        Add("Under Hood", "Leaks", 13);
        Add("Under Hood", "Battery Condition", 14);

        Add("Tires / Brakes", "Tire Tread - Left Front", 20);
        Add("Tires / Brakes", "Tire Tread - Right Front", 21);
        Add("Tires / Brakes", "Tire Tread - Left Rear", 22);
        Add("Tires / Brakes", "Tire Tread - Right Rear", 23);
        Add("Tires / Brakes", "Brake Pad / Shoe Condition - Front", 24);
        Add("Tires / Brakes", "Brake Pad / Shoe Condition - Rear", 25);

        Add("Under Vehicle", "Brake Lines / Hoses", 30);
        Add("Under Vehicle", "Suspension / Steering Components", 31);
        Add("Under Vehicle", "Exhaust System", 32);
        Add("Under Vehicle", "Engine Oil / Fluid Leaks", 33);
        Add("Under Vehicle", "Drive Shaft Boots / CV Boots", 34);

        return items;
    }

    private static AppDbContext CreateContext()
    {
        var factory = new AppDbContextFactory();
        return factory.CreateDbContext(Array.Empty<string>());
    }
}
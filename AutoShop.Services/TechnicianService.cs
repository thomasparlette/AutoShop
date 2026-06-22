using AutoShop.Core.Entities;
using AutoShop.Core.Enums;
using AutoShop.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoShop.Services;

public class TechnicianService
{
    public List<Technician> GetTechnicians(bool includeInactive = false, string? searchText = null)
    {
        using var db = CreateContext();

        var query = db.Technicians.AsNoTracking().AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(t => t.Active);
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            searchText = searchText.Trim();
            query = query.Where(t =>
                EF.Functions.Like(t.FirstName, $"%{searchText}%") ||
                EF.Functions.Like(t.LastName, $"%{searchText}%") ||
                (t.Phone != null && EF.Functions.Like(t.Phone, $"%{searchText}%")));
        }

        return query
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .ToList();
    }

    public Technician? GetTechnicianById(int id)
    {
        using var db = CreateContext();
        return db.Technicians.FirstOrDefault(t => t.Id == id);
    }

    public Technician SaveTechnician(Technician technician)
    {
        using var db = CreateContext();

        if (technician.Id == 0)
        {
            db.Technicians.Add(technician);
        }
        else
        {
            db.Technicians.Update(technician);
        }

        db.SaveChanges();
        return technician;
    }

    public void DeactivateTechnician(int id)
    {
        using var db = CreateContext();

        var tech = db.Technicians.FirstOrDefault(t => t.Id == id);
        if (tech == null)
            return;

        tech.Active = false;
        db.SaveChanges();
    }

    public List<WorkOrder> GetActiveWorkOrdersForTechnician(int technicianId)
    {
        using var db = CreateContext();

        return db.WorkOrders
            .Include(w => w.Customer)
            .Include(w => w.Vehicle)
            .Include(w => w.Technician)
            .AsNoTracking()
            .Where(w =>
                w.TechnicianId == technicianId &&
                (w.Status == WorkOrderStatus.Open ||
                 w.Status == WorkOrderStatus.InProgress ||
                 w.Status == WorkOrderStatus.WaitingApproval))
            .OrderByDescending(w => w.CreatedAt)
            .ToList();
    }

    public List<WorkOrder> GetAllActiveWorkOrders()
    {
        using var db = CreateContext();

        return db.WorkOrders
            .Include(w => w.Customer)
            .Include(w => w.Vehicle)
            .Include(w => w.Technician)
            .AsNoTracking()
            .Where(w =>
                w.Status == WorkOrderStatus.Open ||
                w.Status == WorkOrderStatus.InProgress ||
                w.Status == WorkOrderStatus.WaitingApproval)
            .OrderByDescending(w => w.CreatedAt)
            .ToList();
    }

    private static AppDbContext CreateContext()
    {
        var factory = new AppDbContextFactory();
        return factory.CreateDbContext(Array.Empty<string>());
    }
    public void SyncTechniciansFromUsers()
    {
        using var db = CreateContext();

        var techUsers = db.Users
            .Where(u => u.IsActive && u.Role.HasFlag(UserRole.Technician))
            .ToList();

        foreach (var user in techUsers)
        {
            var displayName = string.IsNullOrWhiteSpace(user.DisplayName)
                ? user.UserName
                : user.DisplayName;

            var parts = displayName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var firstName = parts.Length > 0 ? parts[0] : displayName.Trim();
            var lastName = parts.Length > 1 ? parts[1] : string.Empty;

            var tech = db.Technicians.FirstOrDefault(t => t.UserName == user.UserName);

            if (tech == null)
            {
                db.Technicians.Add(new Technician
                {
                    UserName = user.UserName,
                    FirstName = firstName,
                    LastName = lastName,
                    Phone = null,
                    Active = true,
                    LaborRate = 0m
                });
            }
            else
            {
                tech.FirstName = firstName;
                tech.LastName = lastName;
                tech.Active = true;
            }
        }
        var activeUserNames = techUsers.Select(u => u.UserName).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var inactiveTechs = db.Technicians
            .Where(t => !activeUserNames.Contains(t.UserName))
            .ToList();

        foreach (var tech in inactiveTechs)
        {
            tech.Active = false;
        }
        db.SaveChanges();
    }
}
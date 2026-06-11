using AutoShop.Core.Entities;
using AutoShop.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoShop.Services;

public class VehicleService
{
    public List<Vehicle> GetVehicles(string? searchText = null)
    {
        using var db = CreateContext();

        var query = db.Vehicles
            .Include(v => v.Customer)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            searchText = searchText.Trim();

            query = query.Where(v =>
                EF.Functions.Like(v.Make, $"%{searchText}%") ||
                EF.Functions.Like(v.Model, $"%{searchText}%") ||
                (v.Vin != null && EF.Functions.Like(v.Vin, $"%{searchText}%")) ||
                (v.LicensePlate != null && EF.Functions.Like(v.LicensePlate, $"%{searchText}%")) ||
                (v.Customer != null && EF.Functions.Like(v.Customer.FirstName, $"%{searchText}%")) ||
                (v.Customer != null && EF.Functions.Like(v.Customer.LastName, $"%{searchText}%")));
        }

        return query
            .OrderByDescending(v => v.Year)
            .ThenBy(v => v.Make)
            .ThenBy(v => v.Model)
            .ToList();
    }

    public Vehicle SaveVehicle(Vehicle vehicle)
    {
        using var db = CreateContext();

        if (vehicle.Id == 0)
        {
            db.Vehicles.Add(vehicle);
        }
        else
        {
            db.Vehicles.Update(vehicle);
        }

        db.SaveChanges();
        return vehicle;
    }

    public void DeleteVehicle(int id)
    {
        using var db = CreateContext();

        var vehicle = db.Vehicles.FirstOrDefault(v => v.Id == id);
        if (vehicle == null)
            return;

        db.Vehicles.Remove(vehicle);
        db.SaveChanges();
    }

    public List<Customer> GetCustomers()
    {
        using var db = CreateContext();
        return db.Customers
            .AsNoTracking()
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToList();
    }

    private static AppDbContext CreateContext()
    {
        var factory = new AppDbContextFactory();
        return factory.CreateDbContext(Array.Empty<string>());
    }
}
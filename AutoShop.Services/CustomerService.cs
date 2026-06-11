using AutoShop.Core.Entities;
using AutoShop.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoShop.Services;

public class CustomerService
{
    public List<Customer> GetCustomers(string? searchText = null)
    {
        using var db = CreateContext();

        var query = db.Customers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            searchText = searchText.Trim();

            query = query.Where(c =>
                EF.Functions.Like(c.FirstName, $"%{searchText}%") ||
                EF.Functions.Like(c.LastName, $"%{searchText}%") ||
                EF.Functions.Like(c.Phone, $"%{searchText}%") ||
                (c.Email != null && EF.Functions.Like(c.Email, $"%{searchText}%")));
        }

        return query
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToList();
    }

    public Customer SaveCustomer(Customer customer)
    {
        using var db = CreateContext();

        if (customer.Id == 0)
        {
            db.Customers.Add(customer);
        }
        else
        {
            db.Customers.Update(customer);
        }

        db.SaveChanges();
        return customer;
    }

    public void DeleteCustomer(int id)
    {
        using var db = CreateContext();

        var customer = db.Customers.FirstOrDefault(c => c.Id == id);
        if (customer == null)
            return;

        db.Customers.Remove(customer);
        db.SaveChanges();
    }

    private static AppDbContext CreateContext()
    {
        var factory = new AppDbContextFactory();
        return factory.CreateDbContext(Array.Empty<string>());
    }
}
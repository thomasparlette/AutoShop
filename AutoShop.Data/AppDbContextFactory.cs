using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AutoShop.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private static string GetConnectionString()
    {
        var overrideValue = Environment.GetEnvironmentVariable("AUTOSHOP_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(overrideValue))
            return overrideValue;

        return "Data Source=AutoShop.db";
    }

    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite(GetConnectionString());

        return new AppDbContext(optionsBuilder.Options);
    }
}
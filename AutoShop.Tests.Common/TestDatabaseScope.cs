using AutoShop.Data;
using AutoShop.Services;
using Microsoft.EntityFrameworkCore;

namespace AutoShop.Tests.Common;

public sealed class TestDatabaseScope : IDisposable
{
    private readonly string? _previousConnectionString;

    public string DatabasePath { get; }
    public string ConnectionString { get; }

    public TestDatabaseScope()
    {
        _previousConnectionString = Environment.GetEnvironmentVariable("AUTOSHOP_CONNECTION_STRING");

        DatabasePath = Path.Combine(
            Path.GetTempPath(),
            $"AutoShop.Tests.{Guid.NewGuid():N}.db");

        ConnectionString = $"Data Source={DatabasePath}";
        Environment.SetEnvironmentVariable("AUTOSHOP_CONNECTION_STRING", ConnectionString);

        using var db = new AppDbContextFactory().CreateDbContext(Array.Empty<string>());
        db.Database.Migrate();

        new AuthService().EnsureDefaults();
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("AUTOSHOP_CONNECTION_STRING", _previousConnectionString);

        try
        {
            if (File.Exists(DatabasePath))
                File.Delete(DatabasePath);
        }
        catch
        {
            // ignore cleanup issues
        }
    }
}
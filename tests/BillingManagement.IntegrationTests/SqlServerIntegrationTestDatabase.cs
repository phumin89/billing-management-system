using BillingManagement.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BillingManagement.IntegrationTests;

internal static class SqlServerIntegrationTestDatabase
{
    private const string DefaultConnectionString =
        "Server=localhost,14333;Database=BillingManagement;User Id=sa;Password=Billing_dev_2026!;TrustServerCertificate=True;MultipleActiveResultSets=True";

    public static string CreateDatabaseName() =>
        $"BillingManagementTests_{Guid.NewGuid():N}";

    public static BillingManagementDbContext CreateContext(string databaseName)
    {
        var connectionString = Environment.GetEnvironmentVariable("BMS_TEST_SQLSERVER_CONNECTION") ??
            DefaultConnectionString;
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = databaseName
        };

        var options = new DbContextOptionsBuilder<BillingManagementDbContext>()
            .UseSqlServer(builder.ConnectionString)
            .Options;

        return new BillingManagementDbContext(options);
    }

    public static async Task Delete(string databaseName)
    {
        await using var context = CreateContext(databaseName);
        await context.Database.EnsureDeletedAsync();
    }
}

using BillingManagement.Infrastructure;
using BillingManagement.Migrator;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

var connectionString =
    Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ??
    "Server=localhost,14333;Database=BillingManagement;User Id=sa;Password=Billing_dev_2026!;TrustServerCertificate=True;MultipleActiveResultSets=True";

var options = new DbContextOptionsBuilder<BillingManagementDbContext>()
    .UseSqlServer(connectionString)
    .Options;

await using var dbContext = new BillingManagementDbContext(options);

try
{
    await dbContext.Database.MigrateAsync();
}
catch (SqlException exception) when (SqlServerMigrationErrors.IsDatabaseAlreadyExists(exception))
{
    await dbContext.Database.MigrateAsync();
}

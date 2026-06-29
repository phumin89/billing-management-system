using BillingManagement.Infrastructure;
using Microsoft.EntityFrameworkCore;

var connectionString =
    Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ??
    "Server=localhost,14333;Database=BillingManagement;User Id=sa;Password=Billing_dev_2026!;TrustServerCertificate=True;MultipleActiveResultSets=True";

var options = new DbContextOptionsBuilder<BillingManagementDbContext>()
    .UseSqlServer(connectionString)
    .Options;

await using var dbContext = new BillingManagementDbContext(options);
await dbContext.Database.MigrateAsync();

using BillingManagement.Infrastructure;
using Microsoft.EntityFrameworkCore;

string connectionString =
    Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ??
    "Server=localhost,14333;Database=BillingManagement;User Id=sa;Password=Billing_dev_2026!;TrustServerCertificate=True;MultipleActiveResultSets=True";

DbContextOptions<BillingManagementDbContext> options = new DbContextOptionsBuilder<BillingManagementDbContext>()
    .UseSqlServer(connectionString)
    .Options;

await using BillingManagementDbContext dbContext = new(options);
await dbContext.Database.MigrateAsync();

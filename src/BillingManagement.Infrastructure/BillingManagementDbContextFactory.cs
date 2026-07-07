using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BillingManagement.Infrastructure;

public sealed class BillingManagementDbContextFactory
    : IDesignTimeDbContextFactory<BillingManagementDbContext>
{
    public BillingManagementDbContext CreateDbContext(string[] args)
    {
        string connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ??
            "Server=localhost,14333;Database=BillingManagement;User Id=sa;Password=Billing_dev_2026!;TrustServerCertificate=True;MultipleActiveResultSets=True";

        DbContextOptions<BillingManagementDbContext> options = new DbContextOptionsBuilder<BillingManagementDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new BillingManagementDbContext(options);
    }
}

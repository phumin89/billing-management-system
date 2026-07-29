using BillingManagement.Domain;
using BillingManagement.Infrastructure.Customers;
using BillingManagement.Infrastructure.OwnerCompanyProfiles;
using Microsoft.EntityFrameworkCore;

namespace BillingManagement.Infrastructure;

public sealed class BillingManagementDbContext(DbContextOptions<BillingManagementDbContext> options)
    : DbContext(options)
{
    public DbSet<Customer> Customers => this.Set<Customer>();

    public DbSet<OwnerCompanyProfile> OwnerCompanyProfiles => this.Set<OwnerCompanyProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new OwnerCompanyProfileConfiguration());
    }
}

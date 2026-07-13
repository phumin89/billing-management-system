using BillingManagement.Domain;
using BillingManagement.Infrastructure.OwnerCompanyProfiles;
using Microsoft.EntityFrameworkCore;

namespace BillingManagement.Infrastructure;

public sealed class BillingManagementDbContext(DbContextOptions<BillingManagementDbContext> options)
    : DbContext(options)
{
    public DbSet<OwnerCompanyProfile> OwnerCompanyProfiles => this.Set<OwnerCompanyProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfiguration(new OwnerCompanyProfileConfiguration());
}

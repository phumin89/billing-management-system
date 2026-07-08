using BillingManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace BillingManagement.Infrastructure;

public sealed class BillingManagementDbContext(DbContextOptions<BillingManagementDbContext> options)
    : DbContext(options)
{
    public DbSet<OwnerCompanyProfile> OwnerCompanyProfiles => this.Set<OwnerCompanyProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OwnerCompanyProfile>(entity =>
        {
            entity.HasKey(profile => profile.Id);
            entity.Property(profile => profile.CompanyName).HasMaxLength(200).IsRequired();
            entity.Property(profile => profile.AddressLine1).HasMaxLength(300).IsRequired();
            entity.Property(profile => profile.AddressLine2).HasMaxLength(300);
            entity.Property(profile => profile.CityProvinceState).HasMaxLength(150).IsRequired();
            entity.Property(profile => profile.PostalCode).HasMaxLength(50).IsRequired();
            entity.Property(profile => profile.Country).HasMaxLength(100).IsRequired();
            entity.Property(profile => profile.TaxId).HasMaxLength(100);
            entity.Property(profile => profile.Phone).HasMaxLength(100);
            entity.Property(profile => profile.Email).HasMaxLength(254);
            entity.Property(profile => profile.Website).HasMaxLength(300);
            entity.Property(profile => profile.LogoReference).HasMaxLength(500);
            entity.Property(profile => profile.RegistrationNumber).HasMaxLength(100);
        });
    }
}

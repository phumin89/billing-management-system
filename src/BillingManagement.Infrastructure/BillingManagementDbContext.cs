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
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_SingletonKey", "[SingletonKey] = 1");
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_CompanyName_NotBlank", "LEN(LTRIM(RTRIM([CompanyName]))) > 0");
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_AddressLine1_NotBlank", "LEN(LTRIM(RTRIM([AddressLine1]))) > 0");
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_CityProvinceState_NotBlank", "LEN(LTRIM(RTRIM([CityProvinceState]))) > 0");
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_PostalCode_NotBlank", "LEN(LTRIM(RTRIM([PostalCode]))) > 0");
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_Country_NotBlank", "LEN(LTRIM(RTRIM([Country]))) > 0");
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_AddressLine2_NotBlank", "[AddressLine2] IS NULL OR LEN(LTRIM(RTRIM([AddressLine2]))) > 0");
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_TaxId_NotBlank", "[TaxId] IS NULL OR LEN(LTRIM(RTRIM([TaxId]))) > 0");
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_Phone_NotBlank", "[Phone] IS NULL OR LEN(LTRIM(RTRIM([Phone]))) > 0");
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_Email_NotBlank", "[Email] IS NULL OR LEN(LTRIM(RTRIM([Email]))) > 0");
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_Website_NotBlank", "[Website] IS NULL OR LEN(LTRIM(RTRIM([Website]))) > 0");
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_LogoReference_NotBlank", "[LogoReference] IS NULL OR LEN(LTRIM(RTRIM([LogoReference]))) > 0");
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_RegistrationNumber_NotBlank", "[RegistrationNumber] IS NULL OR LEN(LTRIM(RTRIM([RegistrationNumber]))) > 0");
            });
            entity.HasKey(profile => profile.Id);
            entity.HasIndex(profile => profile.SingletonKey).IsUnique();
            entity.Property(profile => profile.SingletonKey).HasDefaultValue(OwnerCompanyProfile.SingletonKeyValue);
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

using BillingManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace BillingManagement.Infrastructure;

public sealed class BillingManagementDbContext(DbContextOptions<BillingManagementDbContext> options)
    : DbContext(options)
{
    private const string SqlWhitespaceCharacters =
        "N' ' + NCHAR(9) + NCHAR(10) + NCHAR(11) + NCHAR(12) + NCHAR(13) + NCHAR(160)";

    public DbSet<OwnerCompanyProfile> OwnerCompanyProfiles => this.Set<OwnerCompanyProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OwnerCompanyProfile>(entity =>
        {
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_SingletonKey", "[SingletonKey] = 1");
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_CompanyName_NotBlank", RequiredNotBlank("CompanyName"));
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_AddressLine1_NotBlank", RequiredNotBlank("AddressLine1"));
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_CityProvinceState_NotBlank", RequiredNotBlank("CityProvinceState"));
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_PostalCode_NotBlank", RequiredNotBlank("PostalCode"));
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_Country_NotBlank", RequiredNotBlank("Country"));
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_AddressLine2_NotBlank", OptionalNotBlank("AddressLine2"));
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_TaxId_NotBlank", OptionalNotBlank("TaxId"));
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_Phone_NotBlank", OptionalNotBlank("Phone"));
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_Email_NotBlank", OptionalNotBlank("Email"));
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_Website_NotBlank", OptionalNotBlank("Website"));
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_LogoReference_NotBlank", OptionalNotBlank("LogoReference"));
                table.HasCheckConstraint("CK_OwnerCompanyProfiles_RegistrationNumber_NotBlank", OptionalNotBlank("RegistrationNumber"));
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

    private static string RequiredNotBlank(string columnName) =>
        $"LEN(TRIM({SqlWhitespaceCharacters} FROM [{columnName}])) > 0";

    private static string OptionalNotBlank(string columnName) =>
        $"[{columnName}] IS NULL OR {RequiredNotBlank(columnName)}";
}

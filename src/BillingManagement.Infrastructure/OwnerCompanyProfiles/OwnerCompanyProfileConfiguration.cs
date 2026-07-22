using BillingManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillingManagement.Infrastructure.OwnerCompanyProfiles;

public sealed class OwnerCompanyProfileConfiguration
    : IEntityTypeConfiguration<OwnerCompanyProfile>
{
    private const string SingletonKeyProperty = "SingletonKey";
    private const byte SingletonKeyValue = 1;
    private const string SqlWhitespaceCharacters =
        "N' ' + NCHAR(9) + NCHAR(10) + NCHAR(11) + NCHAR(12) + NCHAR(13) + NCHAR(160)";

    public void Configure(EntityTypeBuilder<OwnerCompanyProfile> builder)
    {
        builder.ToTable(table =>
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
            table.HasCheckConstraint("CK_OwnerCompanyProfiles_CoverStorageKey_NotBlank", OptionalNotBlank("CoverStorageKey"));
            table.HasCheckConstraint("CK_OwnerCompanyProfiles_CoverContentType_NotBlank", OptionalNotBlank("CoverContentType"));
            table.HasCheckConstraint("CK_OwnerCompanyProfiles_IconStorageKey_NotBlank", OptionalNotBlank("IconStorageKey"));
            table.HasCheckConstraint("CK_OwnerCompanyProfiles_IconContentType_NotBlank", OptionalNotBlank("IconContentType"));
        });
        builder.HasKey(profile => profile.Id);
        builder.Property<byte>(SingletonKeyProperty).HasDefaultValue(SingletonKeyValue);
        builder.HasIndex(SingletonKeyProperty).IsUnique();
        builder.Property(profile => profile.CompanyName).HasMaxLength(OwnerCompanyProfileConstraints.CompanyNameMaxLength).IsRequired();
        builder.Property(profile => profile.AddressLine1).HasMaxLength(OwnerCompanyProfileConstraints.AddressLine1MaxLength).IsRequired();
        builder.Property(profile => profile.AddressLine2).HasMaxLength(OwnerCompanyProfileConstraints.AddressLine2MaxLength);
        builder.Property(profile => profile.CityProvinceState).HasMaxLength(OwnerCompanyProfileConstraints.CityProvinceStateMaxLength).IsRequired();
        builder.Property(profile => profile.PostalCode).HasMaxLength(OwnerCompanyProfileConstraints.PostalCodeMaxLength).IsRequired();
        builder.Property(profile => profile.Country).HasMaxLength(OwnerCompanyProfileConstraints.CountryMaxLength).IsRequired();
        builder.Property(profile => profile.TaxId).HasMaxLength(OwnerCompanyProfileConstraints.TaxIdMaxLength);
        builder.Property(profile => profile.Phone).HasMaxLength(OwnerCompanyProfileConstraints.PhoneMaxLength);
        builder.Property(profile => profile.Email).HasMaxLength(OwnerCompanyProfileConstraints.EmailMaxLength);
        builder.Property(profile => profile.Website).HasMaxLength(OwnerCompanyProfileConstraints.WebsiteMaxLength);
        builder.Property(profile => profile.LogoReference).HasMaxLength(OwnerCompanyProfileConstraints.LogoReferenceMaxLength);
        builder.Property(profile => profile.RegistrationNumber).HasMaxLength(OwnerCompanyProfileConstraints.RegistrationNumberMaxLength);
        builder.Property(profile => profile.CoverStorageKey).HasMaxLength(OwnerCompanyProfileConstraints.CoverStorageKeyMaxLength);
        builder.Property(profile => profile.CoverContentType).HasMaxLength(OwnerCompanyProfileConstraints.CoverContentTypeMaxLength);
        builder.Property(profile => profile.IconStorageKey).HasMaxLength(OwnerCompanyProfileConstraints.IconStorageKeyMaxLength);
        builder.Property(profile => profile.IconContentType).HasMaxLength(OwnerCompanyProfileConstraints.IconContentTypeMaxLength);
    }

    private static string RequiredNotBlank(string columnName) =>
        $"LEN(TRIM({SqlWhitespaceCharacters} FROM [{columnName}])) > 0";

    private static string OptionalNotBlank(string columnName) =>
        $"[{columnName}] IS NULL OR {RequiredNotBlank(columnName)}";
}

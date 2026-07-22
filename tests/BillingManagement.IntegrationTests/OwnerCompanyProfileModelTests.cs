using BillingManagement.Domain;
using BillingManagement.Infrastructure;
using BillingManagement.Infrastructure.OwnerCompanyProfiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BillingManagement.IntegrationTests;

public sealed class OwnerCompanyProfileModelTests
{
    [Fact]
    public void Model_uses_shared_domain_length_constraints()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(OwnerCompanyProfile))!;
        var expectedLengths = new Dictionary<string, int>
        {
            [nameof(OwnerCompanyProfile.CompanyName)] = OwnerCompanyProfileConstraints.CompanyNameMaxLength,
            [nameof(OwnerCompanyProfile.AddressLine1)] = OwnerCompanyProfileConstraints.AddressLine1MaxLength,
            [nameof(OwnerCompanyProfile.AddressLine2)] = OwnerCompanyProfileConstraints.AddressLine2MaxLength,
            [nameof(OwnerCompanyProfile.CityProvinceState)] = OwnerCompanyProfileConstraints.CityProvinceStateMaxLength,
            [nameof(OwnerCompanyProfile.PostalCode)] = OwnerCompanyProfileConstraints.PostalCodeMaxLength,
            [nameof(OwnerCompanyProfile.Country)] = OwnerCompanyProfileConstraints.CountryMaxLength,
            [nameof(OwnerCompanyProfile.TaxId)] = OwnerCompanyProfileConstraints.TaxIdMaxLength,
            [nameof(OwnerCompanyProfile.Phone)] = OwnerCompanyProfileConstraints.PhoneMaxLength,
            [nameof(OwnerCompanyProfile.Email)] = OwnerCompanyProfileConstraints.EmailMaxLength,
            [nameof(OwnerCompanyProfile.Website)] = OwnerCompanyProfileConstraints.WebsiteMaxLength,
            [nameof(OwnerCompanyProfile.LogoReference)] = OwnerCompanyProfileConstraints.LogoReferenceMaxLength,
            [nameof(OwnerCompanyProfile.RegistrationNumber)] = OwnerCompanyProfileConstraints.RegistrationNumberMaxLength,
            [nameof(OwnerCompanyProfile.CoverStorageKey)] = OwnerCompanyProfileConstraints.CoverStorageKeyMaxLength,
            [nameof(OwnerCompanyProfile.CoverContentType)] = OwnerCompanyProfileConstraints.CoverContentTypeMaxLength,
            [nameof(OwnerCompanyProfile.IconStorageKey)] = OwnerCompanyProfileConstraints.IconStorageKeyMaxLength,
            [nameof(OwnerCompanyProfile.IconContentType)] = OwnerCompanyProfileConstraints.IconContentTypeMaxLength
        };

        foreach (var expected in expectedLengths)
        {
            Assert.Equal(expected.Value, entity.FindProperty(expected.Key)!.GetMaxLength());
        }
    }

    [Fact]
    public void Model_maps_singleton_key_as_infrastructure_only_shadow_property()
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(typeof(OwnerCompanyProfile))!;

        var singletonKey = entity.FindProperty("SingletonKey");

        Assert.NotNull(singletonKey);
        Assert.True(singletonKey.IsShadowProperty());
        Assert.Equal(typeof(byte), singletonKey.ClrType);
        Assert.Equal((byte)1, singletonKey.GetDefaultValue());
        var index = Assert.Single(entity.GetIndexes(), index => index.Properties.Contains(singletonKey));
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void Owner_company_mapping_is_an_extracted_entity_configuration()
    {
        Assert.Contains(
            typeof(IEntityTypeConfiguration<OwnerCompanyProfile>),
            typeof(OwnerCompanyProfileConfiguration).GetInterfaces());
    }

    private static BillingManagementDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BillingManagementDbContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        return new BillingManagementDbContext(options);
    }
}

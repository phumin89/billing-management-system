using BillingManagement.Domain;
using BillingManagement.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BillingManagement.IntegrationTests;

public sealed class CustomerModelTests
{
    [Fact]
    public void Model_maps_customer_schema_without_unique_name_index()
    {
        using var context = CreateContext();
        var entity = context.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(Customer))!;

        Assert.Equal("Customers", entity.GetTableName());
        Assert.Equal(nameof(Customer.Id), Assert.Single(entity.FindPrimaryKey()!.Properties).Name);
        Assert.Empty(entity.GetIndexes());

        AssertProperty(entity, nameof(Customer.CustomerName), CustomerConstraints.CustomerNameMaxLength, false);
        AssertProperty(entity, nameof(Customer.TaxId), CustomerConstraints.TaxIdMaxLength, true);
        AssertProperty(entity, nameof(Customer.Email), CustomerConstraints.EmailMaxLength, true);
        AssertProperty(entity, nameof(Customer.Phone), CustomerConstraints.PhoneMaxLength, true);
        AssertProperty(entity, nameof(Customer.BillingAddressLine1), CustomerConstraints.BillingAddressLine1MaxLength, true);
        AssertProperty(entity, nameof(Customer.BillingAddressLine2), CustomerConstraints.BillingAddressLine2MaxLength, true);
        AssertProperty(entity, nameof(Customer.CityProvinceState), CustomerConstraints.CityProvinceStateMaxLength, true);
        AssertProperty(entity, nameof(Customer.PostalCode), CustomerConstraints.PostalCodeMaxLength, true);
        AssertProperty(entity, nameof(Customer.Country), CustomerConstraints.CountryMaxLength, true);
        AssertProperty(entity, nameof(Customer.ContactName), CustomerConstraints.ContactNameMaxLength, true);
        AssertProperty(entity, nameof(Customer.Notes), CustomerConstraints.NotesMaxLength, true);

        var check = Assert.Single(entity.GetCheckConstraints());
        Assert.Equal("CK_Customers_CustomerName_NotBlank", check.Name);
        Assert.Contains("NCHAR(160)", check.Sql);
    }

    private static void AssertProperty(
        IEntityType entity,
        string name,
        int maximumLength,
        bool nullable)
    {
        var property = entity.FindProperty(name)!;
        Assert.Equal(maximumLength, property.GetMaxLength());
        Assert.Equal(nullable, property.IsNullable);
    }

    private static BillingManagementDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BillingManagementDbContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        return new BillingManagementDbContext(options);
    }
}

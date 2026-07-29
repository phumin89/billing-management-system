using BillingManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillingManagement.Infrastructure.Customers;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    private const string SqlWhitespaceCharacters =
        "N' ' + NCHAR(9) + NCHAR(10) + NCHAR(11) + NCHAR(12) + NCHAR(13) + NCHAR(160)";

    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers", table =>
            table.HasCheckConstraint(
                "CK_Customers_CustomerName_NotBlank",
                $"LEN(TRIM({SqlWhitespaceCharacters} FROM [CustomerName])) > 0"));
        builder.HasKey(customer => customer.Id);
        builder.Property(customer => customer.CustomerName).HasMaxLength(CustomerConstraints.CustomerNameMaxLength).IsRequired();
        builder.Property(customer => customer.TaxId).HasMaxLength(CustomerConstraints.TaxIdMaxLength);
        builder.Property(customer => customer.Email).HasMaxLength(CustomerConstraints.EmailMaxLength);
        builder.Property(customer => customer.Phone).HasMaxLength(CustomerConstraints.PhoneMaxLength);
        builder.Property(customer => customer.BillingAddressLine1).HasMaxLength(CustomerConstraints.BillingAddressLine1MaxLength);
        builder.Property(customer => customer.BillingAddressLine2).HasMaxLength(CustomerConstraints.BillingAddressLine2MaxLength);
        builder.Property(customer => customer.CityProvinceState).HasMaxLength(CustomerConstraints.CityProvinceStateMaxLength);
        builder.Property(customer => customer.PostalCode).HasMaxLength(CustomerConstraints.PostalCodeMaxLength);
        builder.Property(customer => customer.Country).HasMaxLength(CustomerConstraints.CountryMaxLength);
        builder.Property(customer => customer.ContactName).HasMaxLength(CustomerConstraints.ContactNameMaxLength);
        builder.Property(customer => customer.Notes).HasMaxLength(CustomerConstraints.NotesMaxLength);
    }
}

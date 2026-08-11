using BillingManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillingManagement.Infrastructure.BillingDocuments;

public sealed class QuotationConfiguration : IEntityTypeConfiguration<Quotation>
{
    public void Configure(EntityTypeBuilder<Quotation> builder)
    {
        builder.ToTable("Quotations");
        builder.HasKey(quotation => quotation.Id);
        builder.HasIndex(quotation => quotation.Number).IsUnique();
        builder.Property(quotation => quotation.Number).HasMaxLength(50).IsRequired();
        ConfigureSeller(builder);
        builder.Property(quotation => quotation.CustomerName).HasMaxLength(200).IsRequired();
        builder.Property(quotation => quotation.CustomerAddress).HasMaxLength(1000);
        builder.Property(quotation => quotation.CustomerTaxId).HasMaxLength(100);
        builder.Property(quotation => quotation.Currency).HasMaxLength(3).IsFixedLength().IsRequired();
        builder.Ignore(quotation => quotation.Items);
        builder.Ignore(quotation => quotation.Subtotal);
        builder.Ignore(quotation => quotation.TaxTotal);
        builder.Ignore(quotation => quotation.Total);
        builder.HasMany<QuotationItem>("items").WithOne().HasForeignKey("QuotationId").OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureSeller(EntityTypeBuilder<Quotation> builder)
    {
        builder.Property(item => item.SellerCompanyName).HasMaxLength(200).IsRequired();
        builder.Property(item => item.SellerAddress).HasMaxLength(1000).IsRequired();
        builder.Property(item => item.SellerTaxId).HasMaxLength(100);
        builder.Property(item => item.SellerPhone).HasMaxLength(100);
        builder.Property(item => item.SellerEmail).HasMaxLength(320);
        builder.Property(item => item.SellerWebsite).HasMaxLength(500);
        builder.Property(item => item.SellerRegistrationNumber).HasMaxLength(100);
    }
}

using BillingManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillingManagement.Infrastructure.BillingDocuments;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(invoice => invoice.Id);
        builder.HasIndex(invoice => invoice.Number).IsUnique();
        builder.HasIndex(invoice => invoice.QuotationId).IsUnique();
        builder.Property(invoice => invoice.Number).HasMaxLength(50).IsRequired();
        ConfigureSeller(builder);
        builder.Property(invoice => invoice.CustomerName).HasMaxLength(200).IsRequired();
        builder.Property(invoice => invoice.CustomerAddress).HasMaxLength(1000);
        builder.Property(invoice => invoice.CustomerTaxId).HasMaxLength(100);
        builder.Property(invoice => invoice.Currency).HasMaxLength(3).IsFixedLength().IsRequired();
        builder.Property(invoice => invoice.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(invoice => invoice.AmountPaid).HasPrecision(18, 2);
        builder.Ignore(invoice => invoice.Items);
        builder.Ignore(invoice => invoice.Subtotal);
        builder.Ignore(invoice => invoice.TaxTotal);
        builder.Ignore(invoice => invoice.Total);
        builder.HasMany<InvoiceItem>("items").WithOne().HasForeignKey("InvoiceId").OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Quotation>().WithOne().HasForeignKey<Invoice>(invoice => invoice.QuotationId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureSeller(EntityTypeBuilder<Invoice> builder)
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

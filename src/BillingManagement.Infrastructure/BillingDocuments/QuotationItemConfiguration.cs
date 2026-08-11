using BillingManagement.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillingManagement.Infrastructure.BillingDocuments;

public sealed class QuotationItemConfiguration : IEntityTypeConfiguration<QuotationItem>
{
    public void Configure(EntityTypeBuilder<QuotationItem> builder)
    {
        builder.ToTable("QuotationItems");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Description).HasMaxLength(500).IsRequired();
        builder.HasIndex("QuotationId", nameof(QuotationItem.Position)).IsUnique();
        builder.Property(item => item.Quantity).HasPrecision(18, 4);
        builder.Property(item => item.UnitPrice).HasPrecision(18, 2);
        builder.Property(item => item.TaxRate).HasPrecision(5, 2);
        builder.Ignore(item => item.LineSubtotal);
        builder.Ignore(item => item.TaxAmount);
        builder.Ignore(item => item.LineTotal);
    }
}

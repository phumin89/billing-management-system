using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BillingManagement.Infrastructure.CompanyMedia;

internal sealed class CompanyMediaFileConfiguration : IEntityTypeConfiguration<CompanyMediaFile>
{
    public void Configure(EntityTypeBuilder<CompanyMediaFile> builder)
    {
        builder.ToTable("CompanyMedia", table =>
            table.HasCheckConstraint(
                "CK_CompanyMedia_Length",
                "[Length] = DATALENGTH([Content])"));
        builder.HasKey(file => file.Id);
        builder.Property(file => file.Id).ValueGeneratedNever();
        builder.Property(file => file.Content).HasColumnType("varbinary(max)").IsRequired();
        builder.Property(file => file.Length).IsRequired();
        builder.Property(file => file.Version).IsRowVersion();
    }
}

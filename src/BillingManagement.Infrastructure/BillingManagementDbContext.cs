using BillingManagement.Domain;
using BillingManagement.Infrastructure.BillingDocuments;
using BillingManagement.Infrastructure.CompanyMedia;
using BillingManagement.Infrastructure.Customers;
using BillingManagement.Infrastructure.OwnerCompanyProfiles;
using Microsoft.EntityFrameworkCore;

namespace BillingManagement.Infrastructure;

public sealed class BillingManagementDbContext(DbContextOptions<BillingManagementDbContext> options)
    : DbContext(options)
{
    internal DbSet<CompanyMediaFile> CompanyMedia => this.Set<CompanyMediaFile>();

    public DbSet<Invoice> Invoices => this.Set<Invoice>();

    public DbSet<Quotation> Quotations => this.Set<Quotation>();

    public DbSet<Customer> Customers => this.Set<Customer>();

    public DbSet<OwnerCompanyProfile> OwnerCompanyProfiles => this.Set<OwnerCompanyProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CompanyMediaFileConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
        modelBuilder.ApplyConfiguration(new InvoiceItemConfiguration());
        modelBuilder.ApplyConfiguration(new OwnerCompanyProfileConfiguration());
        modelBuilder.ApplyConfiguration(new QuotationConfiguration());
        modelBuilder.ApplyConfiguration(new QuotationItemConfiguration());
    }
}

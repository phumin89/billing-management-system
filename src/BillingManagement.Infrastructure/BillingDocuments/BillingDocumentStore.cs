using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace BillingManagement.Infrastructure.BillingDocuments;

public sealed class BillingDocumentStore(BillingManagementDbContext dbContext) : IBillingDocumentStore
{
    public async Task AddQuotation(Quotation quotation, CancellationToken cancellationToken = default)
    {
        dbContext.Quotations.Add(quotation);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> QuotationNumberExists(string number, CancellationToken cancellationToken = default)
    {
        return dbContext.Quotations.AnyAsync(item => item.Number == number, cancellationToken);
    }

    public async Task<Quotation?> GetQuotationEntity(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Quotations.Include("items").SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public async Task<QuotationRecord?> GetQuotation(Guid id, CancellationToken cancellationToken = default)
    {
        var quotation = await dbContext.Quotations.AsNoTracking().Include("items").SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        return quotation is null ? null : ToRecord(quotation);
    }

    public async Task<IReadOnlyList<QuotationRecord>> ListQuotations(CancellationToken cancellationToken = default)
    {
        var quotations = await dbContext.Quotations.AsNoTracking().Include("items").OrderByDescending(item => item.IssueDate).ThenBy(item => item.Number).ToListAsync(cancellationToken);
        return quotations.Select(ToRecord).ToList();
    }

    public async Task AddInvoice(Invoice invoice, CancellationToken cancellationToken = default)
    {
        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> InvoiceNumberExists(string number, CancellationToken cancellationToken = default)
    {
        return dbContext.Invoices.AnyAsync(item => item.Number == number, cancellationToken);
    }

    public Task<bool> InvoiceExistsForQuotation(Guid quotationId, CancellationToken cancellationToken = default)
    {
        return dbContext.Invoices.AnyAsync(item => item.QuotationId == quotationId, cancellationToken);
    }

    public async Task<InvoiceRecord?> GetInvoice(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await dbContext.Invoices.AsNoTracking().Include("items").SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        return invoice is null ? null : ToRecord(invoice);
    }

    public async Task<IReadOnlyList<InvoiceRecord>> ListInvoices(CancellationToken cancellationToken = default)
    {
        var invoices = await dbContext.Invoices.AsNoTracking().Include("items").OrderByDescending(item => item.IssueDate).ThenBy(item => item.Number).ToListAsync(cancellationToken);
        return invoices.Select(ToRecord).ToList();
    }

    private static QuotationRecord ToRecord(Quotation quotation)
    {
        return new QuotationRecord(quotation.Id, quotation.Number, quotation.SellerCompanyName, quotation.SellerAddress, quotation.SellerTaxId, quotation.SellerPhone, quotation.SellerEmail, quotation.SellerWebsite, quotation.SellerRegistrationNumber, quotation.CustomerId, quotation.CustomerName, quotation.CustomerAddress, quotation.CustomerTaxId, quotation.IssueDate, quotation.ValidUntil, quotation.Currency, quotation.Items.OrderBy(item => item.Position).Select(ToRecord).ToList(), quotation.Subtotal, quotation.TaxTotal, quotation.Total);
    }

    private static InvoiceRecord ToRecord(Invoice invoice)
    {
        return new InvoiceRecord(invoice.Id, invoice.Number, invoice.SellerCompanyName, invoice.SellerAddress, invoice.SellerTaxId, invoice.SellerPhone, invoice.SellerEmail, invoice.SellerWebsite, invoice.SellerRegistrationNumber, invoice.QuotationId, invoice.CustomerId, invoice.CustomerName, invoice.CustomerAddress, invoice.CustomerTaxId, invoice.IssueDate, invoice.DueDate, invoice.Currency, invoice.Items.OrderBy(item => item.Position).Select(ToRecord).ToList(), invoice.Subtotal, invoice.TaxTotal, invoice.Total);
    }

    private static BillingDocumentItemRecord ToRecord(QuotationItem item)
    {
        return new BillingDocumentItemRecord(item.Description, item.Quantity, item.UnitPrice, item.TaxRate);
    }

    private static BillingDocumentItemRecord ToRecord(InvoiceItem item)
    {
        return new BillingDocumentItemRecord(item.Description, item.Quantity, item.UnitPrice, item.TaxRate);
    }
}

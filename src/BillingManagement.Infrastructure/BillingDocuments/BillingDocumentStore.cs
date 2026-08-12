using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.Queries;
using BillingManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace BillingManagement.Infrastructure.BillingDocuments;

public sealed class BillingDocumentStore(BillingManagementDbContext dbContext) : IBillingDocumentStore, IBillingDocumentQueries
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

    public async Task<BillingDocumentPage<QuotationRecord>> SearchQuotations(
        QuotationSearchCriteria criteria,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = Math.Max(page.PageNumber, 1);
        var pageSize = Math.Clamp(page.PageSize, 1, 100);
        var matching = ApplyQuotationSearch(dbContext.Quotations.AsNoTracking(), criteria);
        var totalCount = await matching.CountAsync(cancellationToken);
        var quotations = await matching
            .OrderByDescending(item => item.IssueDate)
            .ThenBy(item => item.Number)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include("items")
            .ToListAsync(cancellationToken);
        return new BillingDocumentPage<QuotationRecord>(
            quotations.Select(ToRecord).ToList(), pageNumber, pageSize, totalCount);
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

    public Task<Invoice?> GetInvoiceEntity(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Invoices.Include("items").SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public async Task SaveInvoice(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InvoiceRecord>> ListInvoices(CancellationToken cancellationToken = default)
    {
        var invoices = await dbContext.Invoices.AsNoTracking().Include("items").OrderByDescending(item => item.IssueDate).ThenBy(item => item.Number).ToListAsync(cancellationToken);
        return invoices.Select(ToRecord).ToList();
    }

    public async Task<BillingDocumentPage<InvoiceRecord>> SearchInvoices(
        InvoiceSearchCriteria criteria,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = Math.Max(page.PageNumber, 1);
        var pageSize = Math.Clamp(page.PageSize, 1, 100);
        var matching = ApplyInvoiceSearch(dbContext.Invoices.AsNoTracking(), criteria);
        var totalCount = await matching.CountAsync(cancellationToken);
        var invoices = await matching
            .OrderByDescending(item => item.IssueDate)
            .ThenBy(item => item.Number)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include("items")
            .ToListAsync(cancellationToken);
        return new BillingDocumentPage<InvoiceRecord>(
            invoices.Select(ToRecord).ToList(), pageNumber, pageSize, totalCount);
    }

    public async Task<InvoiceDashboardRecord> GetInvoiceDashboard(
        DateOnly today,
        CancellationToken cancellationToken = default)
    {
        var invoices = await dbContext.Invoices
            .AsNoTracking()
            .Include("items")
            .OrderByDescending(item => item.IssueDate)
            .ThenBy(item => item.Number)
            .ToListAsync(cancellationToken);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var nextMonth = monthStart.AddMonths(1);

        return new InvoiceDashboardRecord(
            GroupTotals(invoices.Where(item => item.Status == InvoiceStatus.Issued)),
            GroupTotals(invoices.Where(item =>
                item.Status == InvoiceStatus.Paid &&
                item.PaidDate >= monthStart && item.PaidDate < nextMonth)),
            GroupTotals(invoices.Where(item =>
                item.Status == InvoiceStatus.Issued && item.DueDate < today)),
            invoices.Take(5).Select(ToRecord).ToList());
    }

    private static IReadOnlyList<InvoiceCurrencyTotalRecord> GroupTotals(
        IEnumerable<Invoice> invoices)
    {
        return invoices
            .GroupBy(item => item.Currency)
            .OrderBy(group => group.Key)
            .Select(group => new InvoiceCurrencyTotalRecord(
                group.Key, group.Count(), group.Sum(item => item.Total)))
            .ToList();
    }

    private static IQueryable<Quotation> ApplyQuotationSearch(
        IQueryable<Quotation> quotations,
        QuotationSearchCriteria criteria)
    {
        if (string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            return quotations;
        }

        var searchText = criteria.SearchText.Trim();
        return quotations.Where(item =>
            item.Number.Contains(searchText) || item.CustomerName.Contains(searchText));
    }

    private static IQueryable<Invoice> ApplyInvoiceSearch(
        IQueryable<Invoice> invoices,
        InvoiceSearchCriteria criteria)
    {
        if (!string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            var searchText = criteria.SearchText.Trim();
            invoices = invoices.Where(item =>
                item.Number.Contains(searchText) || item.CustomerName.Contains(searchText));
        }

        if (criteria.Status is null)
        {
            return invoices;
        }

        var today = criteria.Today ?? DateOnly.FromDateTime(DateTime.UtcNow);
        return criteria.Status switch
        {
            InvoiceStatus.Overdue => invoices.Where(item =>
                item.Status == InvoiceStatus.Issued && item.DueDate < today),
            InvoiceStatus.Issued => invoices.Where(item =>
                item.Status == InvoiceStatus.Issued && item.DueDate >= today),
            _ => invoices.Where(item => item.Status == criteria.Status)
        };
    }

    private static QuotationRecord ToRecord(Quotation quotation)
    {
        return new QuotationRecord(quotation.Id, quotation.Number, quotation.SellerCompanyName, quotation.SellerAddress, quotation.SellerTaxId, quotation.SellerPhone, quotation.SellerEmail, quotation.SellerWebsite, quotation.SellerRegistrationNumber, quotation.CustomerId, quotation.CustomerName, quotation.CustomerAddress, quotation.CustomerTaxId, quotation.IssueDate, quotation.ValidUntil, quotation.Currency, quotation.Items.OrderBy(item => item.Position).Select(ToRecord).ToList(), quotation.Subtotal, quotation.TaxTotal, quotation.Total);
    }

    private static InvoiceRecord ToRecord(Invoice invoice)
    {
        return new InvoiceRecord(invoice.Id, invoice.Number, invoice.SellerCompanyName, invoice.SellerAddress, invoice.SellerTaxId, invoice.SellerPhone, invoice.SellerEmail, invoice.SellerWebsite, invoice.SellerRegistrationNumber, invoice.QuotationId, invoice.CustomerId, invoice.CustomerName, invoice.CustomerAddress, invoice.CustomerTaxId, invoice.IssueDate, invoice.DueDate, invoice.Currency, invoice.Items.OrderBy(item => item.Position).Select(ToRecord).ToList(), invoice.Subtotal, invoice.TaxTotal, invoice.Total, invoice.Status, invoice.PaidDate, invoice.AmountPaid);
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

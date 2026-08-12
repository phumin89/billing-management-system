using BillingManagement.Application.Abstractions.Queries;

namespace BillingManagement.Application.Abstractions.BillingDocuments;

public interface IBillingDocumentQueries
{
    Task<BillingDocumentPage<QuotationRecord>> SearchQuotations(
        QuotationSearchCriteria criteria,
        PageRequest page,
        CancellationToken cancellationToken = default);

    Task<BillingDocumentPage<InvoiceRecord>> SearchInvoices(
        InvoiceSearchCriteria criteria,
        PageRequest page,
        CancellationToken cancellationToken = default);

    Task<InvoiceDashboardRecord> GetInvoiceDashboard(
        DateOnly today,
        CancellationToken cancellationToken = default);
}

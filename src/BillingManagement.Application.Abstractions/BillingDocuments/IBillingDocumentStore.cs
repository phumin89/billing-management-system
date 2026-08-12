using BillingManagement.Domain;

namespace BillingManagement.Application.Abstractions.BillingDocuments;

public interface IBillingDocumentStore
{
    Task AddQuotation(Quotation quotation, CancellationToken cancellationToken = default);
    Task<bool> QuotationNumberExists(string number, CancellationToken cancellationToken = default);
    Task<Quotation?> GetQuotationEntity(Guid id, CancellationToken cancellationToken = default);
    Task<QuotationRecord?> GetQuotation(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QuotationRecord>> ListQuotations(CancellationToken cancellationToken = default);
    Task AddInvoice(Invoice invoice, CancellationToken cancellationToken = default);
    Task<bool> InvoiceNumberExists(string number, CancellationToken cancellationToken = default);
    Task<bool> InvoiceExistsForQuotation(Guid quotationId, CancellationToken cancellationToken = default);
    Task<InvoiceRecord?> GetInvoice(Guid id, CancellationToken cancellationToken = default);
    Task<Invoice?> GetInvoiceEntity(Guid id, CancellationToken cancellationToken = default);
    Task SaveInvoice(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InvoiceRecord>> ListInvoices(CancellationToken cancellationToken = default);
}

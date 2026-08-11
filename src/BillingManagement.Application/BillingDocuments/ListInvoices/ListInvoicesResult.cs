using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.Results;

namespace BillingManagement.Application.BillingDocuments.ListInvoices;

public sealed record ListInvoicesResult(IReadOnlyList<InvoiceRecord> Items) : IQueryResult;

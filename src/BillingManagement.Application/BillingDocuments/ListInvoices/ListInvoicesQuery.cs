using BillingManagement.Application.Abstractions.Queries;

using BillingManagement.Domain;

namespace BillingManagement.Application.BillingDocuments.ListInvoices;

public sealed record ListInvoicesQuery(
    string? SearchText = null,
    InvoiceStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 20,
    DateOnly? Today = null) : IQuery<ListInvoicesResult>;

using BillingManagement.Application.Abstractions.Queries;

namespace BillingManagement.Application.BillingDocuments.ListInvoices;

public sealed record ListInvoicesQuery : IQuery<ListInvoicesResult>;

using BillingManagement.Application.Abstractions.Queries;

namespace BillingManagement.Application.BillingDocuments.GetInvoice;

public sealed record GetInvoiceQuery(Guid Id) : IQuery<GetInvoiceResult>;

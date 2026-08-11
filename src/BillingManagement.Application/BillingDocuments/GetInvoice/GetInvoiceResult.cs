using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.Results;

namespace BillingManagement.Application.BillingDocuments.GetInvoice;

public sealed record GetInvoiceResult(InvoiceRecord? Invoice) : IQueryResult;

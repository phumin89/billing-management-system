using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.Results;

namespace BillingManagement.Application.BillingDocuments.GetQuotation;

public sealed record GetQuotationResult(QuotationRecord? Quotation) : IQueryResult;

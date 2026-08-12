using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.Results;

namespace BillingManagement.Application.BillingDocuments.ListQuotations;

public sealed record ListQuotationsResult(
    IReadOnlyList<QuotationRecord> Items,
    int PageNumber,
    int PageSize,
    int TotalCount) : IQueryResult;

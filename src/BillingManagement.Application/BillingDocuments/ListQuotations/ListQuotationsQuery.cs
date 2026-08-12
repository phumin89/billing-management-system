using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.Queries;

namespace BillingManagement.Application.BillingDocuments.ListQuotations;

public sealed record ListQuotationsQuery(
    string? SearchText = null,
    int PageNumber = 1,
    int PageSize = 20) : IQuery<ListQuotationsResult>;

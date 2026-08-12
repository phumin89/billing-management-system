namespace BillingManagement.Application.Abstractions.BillingDocuments;

public sealed record BillingDocumentPage<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount);

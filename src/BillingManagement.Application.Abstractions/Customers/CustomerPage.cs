namespace BillingManagement.Application.Abstractions.Customers;

public sealed record CustomerPage(
    IReadOnlyList<CustomerRecord> Items,
    int PageNumber,
    int PageSize,
    int TotalCount);

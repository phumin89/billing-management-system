using BillingManagement.Contracts.Customers;

namespace BillingManagement.Client.Customers;

public sealed record ListCustomersResult(
    bool Succeeded,
    IReadOnlyList<CustomerResponse> Customers,
    string? Message = null,
    int PageNumber = 1,
    int PageSize = 100,
    int TotalCount = 0)
{
    public static ListCustomersResult Success(
        IReadOnlyList<CustomerResponse> customers,
        int pageNumber = 1,
        int pageSize = 100,
        int? totalCount = null) =>
        new(true, customers, null, pageNumber, pageSize, totalCount ?? customers.Count);

    public static ListCustomersResult Failed(string message) =>
        new(false, [], message);
}

using BillingManagement.Contracts.Customers;

namespace BillingManagement.Client.Customers;

public sealed record ListCustomersResult(
    bool Succeeded,
    IReadOnlyList<CustomerResponse> Customers,
    string? Message = null)
{
    public static ListCustomersResult Success(IReadOnlyList<CustomerResponse> customers) =>
        new(true, customers);

    public static ListCustomersResult Failed(string message) =>
        new(false, [], message);
}

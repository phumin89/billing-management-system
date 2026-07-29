using BillingManagement.Contracts.Customers;

namespace BillingManagement.Client.Customers;

public sealed record CreateCustomerResult(
    bool Succeeded,
    CustomerResponse? Customer,
    IReadOnlyDictionary<string, string[]> Errors,
    string? Message = null)
{
    public static CreateCustomerResult Success(CustomerResponse customer) =>
        new(true, customer, new Dictionary<string, string[]>());

    public static CreateCustomerResult Failed(
        IReadOnlyDictionary<string, string[]> errors,
        string? message = null) =>
        new(false, null, errors, message);
}

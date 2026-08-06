namespace BillingManagement.Client.Customers;

public sealed record DeleteCustomerResult(bool ShouldRemoveCustomer, string? Message = null)
{
    public static DeleteCustomerResult Removed(string? message = null) => new(true, message);

    public static DeleteCustomerResult Failed(string message) => new(false, message);
}

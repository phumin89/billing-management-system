namespace BillingManagement.Contracts.Customers;

public sealed class UpdateCustomerRequest
{
    public string? CustomerName { get; set; }

    public string? TaxId { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? BillingAddressLine1 { get; set; }

    public string? BillingAddressLine2 { get; set; }

    public string? CityProvinceState { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    public string? ContactName { get; set; }

    public string? Notes { get; set; }
}

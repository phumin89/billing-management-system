namespace BillingManagement.Application.Abstractions.Customers;

public sealed record CustomerRecord(
    Guid Id,
    string CustomerName,
    string? TaxId,
    string? Email,
    string? Phone,
    string? BillingAddressLine1,
    string? BillingAddressLine2,
    string? CityProvinceState,
    string? PostalCode,
    string? Country,
    string? ContactName,
    string? Notes);

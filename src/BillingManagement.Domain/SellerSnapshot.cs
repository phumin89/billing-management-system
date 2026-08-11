namespace BillingManagement.Domain;

public sealed record SellerSnapshot(
    string CompanyName,
    string Address,
    string? TaxId,
    string? Phone,
    string? Email,
    string? Website,
    string? RegistrationNumber);

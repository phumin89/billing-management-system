namespace BillingManagement.Application.Abstractions.OwnerCompanyProfiles;

public sealed record OwnerCompanyProfileRecord(
    Guid Id,
    string CompanyName,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string PostalCode,
    string Country,
    string? TaxId,
    string? Phone,
    string? Email,
    string? Website,
    string? LogoReference,
    string? RegistrationNumber);

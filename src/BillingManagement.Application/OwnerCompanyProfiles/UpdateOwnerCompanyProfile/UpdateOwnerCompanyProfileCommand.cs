namespace BillingManagement.Application.OwnerCompanyProfiles.UpdateOwnerCompanyProfile;

public sealed record UpdateOwnerCompanyProfileCommand(
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

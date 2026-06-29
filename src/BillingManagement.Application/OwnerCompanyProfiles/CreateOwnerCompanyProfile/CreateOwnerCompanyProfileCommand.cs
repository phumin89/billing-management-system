namespace BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;

public sealed record CreateOwnerCompanyProfileCommand(
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

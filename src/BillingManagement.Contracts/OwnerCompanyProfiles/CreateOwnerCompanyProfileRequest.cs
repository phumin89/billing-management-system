namespace BillingManagement.Contracts.OwnerCompanyProfiles;

public sealed class CreateOwnerCompanyProfileRequest
{
    public string? CompanyName { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? CityProvinceState { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    public string? TaxId { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Website { get; set; }

    public string? LogoReference { get; set; }

    public string? RegistrationNumber { get; set; }
}

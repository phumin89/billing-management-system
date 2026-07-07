using System.ComponentModel.DataAnnotations;

namespace BillingManagement.Contracts.OwnerCompanyProfiles;

public sealed class CreateOwnerCompanyProfileRequest
{
    [Required(ErrorMessage = "Company name is required.")]
    public string? CompanyName { get; set; }

    [Required(ErrorMessage = "Address line 1 is required.")]
    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    [Required(ErrorMessage = "City / province / state is required.")]
    public string? CityProvinceState { get; set; }

    [Required(ErrorMessage = "Postal code is required.")]
    public string? PostalCode { get; set; }

    [Required(ErrorMessage = "Country is required.")]
    public string? Country { get; set; }

    public string? TaxId { get; set; }

    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Email format is invalid.")]
    public string? Email { get; set; }

    public string? Website { get; set; }

    public string? LogoReference { get; set; }

    public string? RegistrationNumber { get; set; }
}

using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Validation;
using BillingManagement.Domain;

namespace BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;

public sealed class CreateOwnerCompanyProfileValidator
    : ICommandValidator<CreateOwnerCompanyProfileCommand>
{
    public IReadOnlyDictionary<string, string[]> Validate(CreateOwnerCompanyProfileCommand command)
    {
        var errors = new Dictionary<string, string[]>();
        errors.AddRequired(nameof(command.CompanyName), command.CompanyName, "Company name is required.");
        errors.AddRequired(nameof(command.AddressLine1), command.AddressLine1, "Address line 1 is required.");
        errors.AddRequired(nameof(command.City), command.City, "City / province / state is required.");
        errors.AddRequired(nameof(command.PostalCode), command.PostalCode, "Postal code is required.");
        errors.AddRequired(nameof(command.Country), command.Country, "Country is required.");
        errors.AddMaxLength(nameof(command.CompanyName), command.CompanyName, OwnerCompanyProfile.CompanyNameMaxLength);
        errors.AddMaxLength(nameof(command.AddressLine1), command.AddressLine1, OwnerCompanyProfile.AddressLine1MaxLength);
        errors.AddMaxLength(nameof(command.AddressLine2), command.AddressLine2, OwnerCompanyProfile.AddressLine2MaxLength);
        errors.AddMaxLength(nameof(command.City), command.City, OwnerCompanyProfile.CityProvinceStateMaxLength);
        errors.AddMaxLength(nameof(command.PostalCode), command.PostalCode, OwnerCompanyProfile.PostalCodeMaxLength);
        errors.AddMaxLength(nameof(command.Country), command.Country, OwnerCompanyProfile.CountryMaxLength);
        errors.AddMaxLength(nameof(command.TaxId), command.TaxId, OwnerCompanyProfile.TaxIdMaxLength);
        errors.AddMaxLength(nameof(command.Phone), command.Phone, OwnerCompanyProfile.PhoneMaxLength);
        errors.AddMaxLength(nameof(command.Email), command.Email, OwnerCompanyProfile.EmailMaxLength);
        errors.AddMaxLength(nameof(command.Website), command.Website, OwnerCompanyProfile.WebsiteMaxLength);
        errors.AddMaxLength(nameof(command.LogoReference), command.LogoReference, OwnerCompanyProfile.LogoReferenceMaxLength);
        errors.AddMaxLength(nameof(command.RegistrationNumber), command.RegistrationNumber, OwnerCompanyProfile.RegistrationNumberMaxLength);
        errors.AddEmail(nameof(command.Email), command.Email);
        return errors;
    }
}

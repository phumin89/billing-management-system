using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Domain;

namespace BillingManagement.Application.OwnerCompanyProfiles.UpdateOwnerCompanyProfile;

public sealed class UpdateOwnerCompanyProfileHandler(
    IOwnerCompanyProfileStore store)
    : ICommandHandler<UpdateOwnerCompanyProfileCommand, UpdateOwnerCompanyProfileResult>
{
    public async Task<UpdateOwnerCompanyProfileResult> Handle(
        UpdateOwnerCompanyProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        var errors = Validate(command);
        if (errors.Count > 0)
        {
            return UpdateOwnerCompanyProfileResult.Failed(errors);
        }

        var existingProfile = await store.GetAsync(cancellationToken);
        if (existingProfile is null)
        {
            return UpdateOwnerCompanyProfileResult.Missing();
        }

        var profile = new OwnerCompanyProfileRecord(
            existingProfile.Id,
            command.CompanyName.Trim(),
            command.AddressLine1.Trim(),
            BlankToNull(command.AddressLine2),
            command.City.Trim(),
            command.PostalCode.Trim(),
            command.Country.Trim(),
            BlankToNull(command.TaxId),
            BlankToNull(command.Phone),
            BlankToNull(command.Email),
            BlankToNull(command.Website),
            BlankToNull(command.LogoReference),
            BlankToNull(command.RegistrationNumber));

        if (!await store.Update(profile, cancellationToken))
        {
            return UpdateOwnerCompanyProfileResult.Missing();
        }

        return UpdateOwnerCompanyProfileResult.Success(profile);
    }

    private static Dictionary<string, string[]> Validate(UpdateOwnerCompanyProfileCommand command)
    {
        var errors = new Dictionary<string, string[]>();
        AddRequired(errors, nameof(command.CompanyName), command.CompanyName, "Company name is required.");
        AddRequired(errors, nameof(command.AddressLine1), command.AddressLine1, "Address line 1 is required.");
        AddRequired(errors, nameof(command.City), command.City, "City / province / state is required.");
        AddRequired(errors, nameof(command.PostalCode), command.PostalCode, "Postal code is required.");
        AddRequired(errors, nameof(command.Country), command.Country, "Country is required.");
        AddMaxLength(errors, nameof(command.CompanyName), command.CompanyName, OwnerCompanyProfile.CompanyNameMaxLength);
        AddMaxLength(errors, nameof(command.AddressLine1), command.AddressLine1, OwnerCompanyProfile.AddressLine1MaxLength);
        AddMaxLength(errors, nameof(command.AddressLine2), command.AddressLine2, OwnerCompanyProfile.AddressLine2MaxLength);
        AddMaxLength(errors, nameof(command.City), command.City, OwnerCompanyProfile.CityProvinceStateMaxLength);
        AddMaxLength(errors, nameof(command.PostalCode), command.PostalCode, OwnerCompanyProfile.PostalCodeMaxLength);
        AddMaxLength(errors, nameof(command.Country), command.Country, OwnerCompanyProfile.CountryMaxLength);
        AddMaxLength(errors, nameof(command.TaxId), command.TaxId, OwnerCompanyProfile.TaxIdMaxLength);
        AddMaxLength(errors, nameof(command.Phone), command.Phone, OwnerCompanyProfile.PhoneMaxLength);
        AddMaxLength(errors, nameof(command.Email), command.Email, OwnerCompanyProfile.EmailMaxLength);
        AddMaxLength(errors, nameof(command.Website), command.Website, OwnerCompanyProfile.WebsiteMaxLength);
        AddMaxLength(errors, nameof(command.LogoReference), command.LogoReference, OwnerCompanyProfile.LogoReferenceMaxLength);
        AddMaxLength(errors, nameof(command.RegistrationNumber), command.RegistrationNumber, OwnerCompanyProfile.RegistrationNumberMaxLength);

        if (!string.IsNullOrWhiteSpace(command.Email) &&
            !command.Email.Contains('@', StringComparison.Ordinal))
        {
            errors[nameof(command.Email)] = ["Email format is invalid."];
        }

        return errors;
    }

    private static void AddRequired(
        Dictionary<string, string[]> errors,
        string fieldName,
        string? value,
        string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[fieldName] = [message];
        }
    }

    private static string? BlankToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void AddMaxLength(
        Dictionary<string, string[]> errors,
        string fieldName,
        string? value,
        int maxLength)
    {
        if (value?.Trim().Length > maxLength)
        {
            errors[fieldName] = [$"Must not exceed {maxLength} characters."];
        }
    }
}

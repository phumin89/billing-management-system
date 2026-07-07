using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;

namespace BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;

public sealed class CreateOwnerCompanyProfileHandler(
    IOwnerCompanyProfileStore store)
    : ICommandHandler<CreateOwnerCompanyProfileCommand, CreateOwnerCompanyProfileResult>
{
    public async Task<CreateOwnerCompanyProfileResult> Handle(
        CreateOwnerCompanyProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        Dictionary<string, string[]> errors = Validate(command);
        if (errors.Count > 0)
        {
            return CreateOwnerCompanyProfileResult.Failed(errors);
        }

        if (await store.GetAsync(cancellationToken) is not null)
        {
            return CreateOwnerCompanyProfileResult.Failed(new Dictionary<string, string[]>
            {
                ["Profile"] = ["Owner company profile already exists."]
            });
        }

        OwnerCompanyProfileRecord profile = new(
            Guid.NewGuid(),
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

        await store.Add(profile, cancellationToken);

        return CreateOwnerCompanyProfileResult.Success(profile);
    }

    private static Dictionary<string, string[]> Validate(CreateOwnerCompanyProfileCommand command)
    {
        Dictionary<string, string[]> errors = [];
        AddRequired(errors, nameof(command.CompanyName), command.CompanyName, "Company name is required.");
        AddRequired(errors, nameof(command.AddressLine1), command.AddressLine1, "Address line 1 is required.");
        AddRequired(errors, nameof(command.City), command.City, "City / province / state is required.");
        AddRequired(errors, nameof(command.PostalCode), command.PostalCode, "Postal code is required.");
        AddRequired(errors, nameof(command.Country), command.Country, "Country is required.");

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
}

using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Domain;

namespace BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;

public sealed class CreateOwnerCompanyProfileHandler(
    IOwnerCompanyProfileStore store)
    : ICommandHandler<CreateOwnerCompanyProfileCommand, CreateOwnerCompanyProfileResult>
{
    public async Task<CreateOwnerCompanyProfileResult> Handle(
        CreateOwnerCompanyProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        if (await store.GetAsync(cancellationToken) is not null)
        {
            return CreateOwnerCompanyProfileResult.Failed(new Dictionary<string, string[]>
            {
                ["Profile"] = ["Owner company profile already exists."]
            });
        }

        var ownerCompanyProfile = OwnerCompanyProfile.Create(
            Guid.NewGuid(),
            command.CompanyName,
            command.AddressLine1,
            command.AddressLine2,
            command.City,
            command.PostalCode,
            command.Country,
            command.TaxId,
            command.Phone,
            command.Email,
            command.Website,
            command.LogoReference,
            command.RegistrationNumber);
        var profile = ToRecord(ownerCompanyProfile);

        if (!await store.Add(profile, cancellationToken))
        {
            return CreateOwnerCompanyProfileResult.Failed(new Dictionary<string, string[]>
            {
                ["Profile"] = ["Owner company profile already exists."]
            });
        }

        return CreateOwnerCompanyProfileResult.Success(profile);
    }

    private static OwnerCompanyProfileRecord ToRecord(OwnerCompanyProfile profile) =>
        new(
            profile.Id,
            profile.CompanyName,
            profile.AddressLine1,
            profile.AddressLine2,
            profile.CityProvinceState,
            profile.PostalCode,
            profile.Country,
            profile.TaxId,
            profile.Phone,
            profile.Email,
            profile.Website,
            profile.LogoReference,
            profile.RegistrationNumber);
}

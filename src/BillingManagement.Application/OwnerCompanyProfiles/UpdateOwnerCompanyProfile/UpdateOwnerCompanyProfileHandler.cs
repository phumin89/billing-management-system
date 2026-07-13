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
        var existingProfile = await store.GetAsync(cancellationToken);
        if (existingProfile is null)
        {
            return UpdateOwnerCompanyProfileResult.Missing();
        }

        var ownerCompanyProfile = OwnerCompanyProfile.Create(
            existingProfile.Id,
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

        if (!await store.Update(profile, cancellationToken))
        {
            return UpdateOwnerCompanyProfileResult.Missing();
        }

        return UpdateOwnerCompanyProfileResult.Success(profile);
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

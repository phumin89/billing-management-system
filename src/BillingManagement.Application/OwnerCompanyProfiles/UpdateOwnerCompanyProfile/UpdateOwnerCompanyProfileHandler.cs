using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.Abstractions.Results;
using BillingManagement.Domain;

namespace BillingManagement.Application.OwnerCompanyProfiles.UpdateOwnerCompanyProfile;

public sealed class UpdateOwnerCompanyProfileHandler(
    IOwnerCompanyProfileStore store)
    : ICommandHandler<UpdateOwnerCompanyProfileCommand, OwnerCompanyProfileRecord>
{
    public async Task<ApplicationResult<OwnerCompanyProfileRecord>> Handle(
        UpdateOwnerCompanyProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        var existingProfile = await store.GetAsync(cancellationToken);
        if (existingProfile is null)
        {
            return MissingProfile();
        }

        var ownerCompanyProfile = OwnerCompanyProfile.Rehydrate(
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
            return MissingProfile();
        }

        return ApplicationResult<OwnerCompanyProfileRecord>.Success(profile);
    }

    private static ApplicationResult<OwnerCompanyProfileRecord> MissingProfile() =>
        ApplicationResult<OwnerCompanyProfileRecord>.Failure(ApplicationError.NotFound(
            "owner_company_profile.not_found",
            "Owner company profile was not found."));

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

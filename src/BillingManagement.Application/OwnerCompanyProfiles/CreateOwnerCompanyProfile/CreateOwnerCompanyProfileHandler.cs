using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.Abstractions.Results;
using BillingManagement.Domain;

namespace BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;

public sealed class CreateOwnerCompanyProfileHandler(
    IOwnerCompanyProfileStore store)
    : ICommandHandler<CreateOwnerCompanyProfileCommand, OwnerCompanyProfileRecord>
{
    public async Task<ApplicationResult<OwnerCompanyProfileRecord>> Handle(
        CreateOwnerCompanyProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        if (await store.GetAsync(cancellationToken) is not null)
        {
            return DuplicateProfile();
        }

        var ownerCompanyProfile = OwnerCompanyProfile.Create(
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
            return DuplicateProfile();
        }

        return ApplicationResult<OwnerCompanyProfileRecord>.Success(profile);
    }

    private static ApplicationResult<OwnerCompanyProfileRecord> DuplicateProfile() =>
        ApplicationResult<OwnerCompanyProfileRecord>.Failure(ApplicationError.Conflict(
            "owner_company_profile.already_exists",
            "Owner company profile already exists."));

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

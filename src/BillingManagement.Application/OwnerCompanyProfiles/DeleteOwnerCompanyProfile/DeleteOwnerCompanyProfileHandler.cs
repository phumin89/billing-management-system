using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.CompanyMedia;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.Abstractions.Results;
using Microsoft.Extensions.Logging;

namespace BillingManagement.Application.OwnerCompanyProfiles.DeleteOwnerCompanyProfile;

public sealed class DeleteOwnerCompanyProfileHandler(
    IOwnerCompanyProfileStore store,
    ICompanyMediaStore mediaStore,
    ILogger<DeleteOwnerCompanyProfileHandler> logger)
    : ICommandHandler<DeleteOwnerCompanyProfileCommand, bool>
{
    public async Task<ApplicationResult<bool>> Handle(
        DeleteOwnerCompanyProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        var profile = await store.GetAsync(cancellationToken);
        var result = await store.Delete(cancellationToken);

        return result switch
        {
            OwnerCompanyProfileDeleteResult.Deleted => await this.DeleteMediaAsync(profile, cancellationToken),
            OwnerCompanyProfileDeleteResult.NotFound => MissingProfile(),
            OwnerCompanyProfileDeleteResult.DependencyConflict => ProfileInUse(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, "Unsupported delete result.")
        };
    }

    private async Task<ApplicationResult<bool>> DeleteMediaAsync(
        OwnerCompanyProfileRecord? profile,
        CancellationToken cancellationToken)
    {
        if (profile is null)
        {
            return ApplicationResult<bool>.Success(true);
        }

        await this.DeleteMediaFileAsync(profile.CoverStorageKey, "cover", cancellationToken);
        await this.DeleteMediaFileAsync(profile.IconStorageKey, "icon", cancellationToken);
        return ApplicationResult<bool>.Success(true);
    }

    private async Task DeleteMediaFileAsync(
        string? storageKey,
        string mediaType,
        CancellationToken cancellationToken)
    {
        if (storageKey is null)
        {
            return;
        }

        try
        {
            await mediaStore.DeleteAsync(CompanyMediaStorageKey.Parse(storageKey), cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Could not delete company profile {MediaType} media with storage key {StorageKey}.",
                mediaType,
                storageKey);
        }
    }

    private static ApplicationResult<bool> MissingProfile() =>
        ApplicationResult<bool>.Failure(ApplicationError.NotFound(
            "owner_company_profile.not_found",
            "Owner company profile was not found."));

    private static ApplicationResult<bool> ProfileInUse() =>
        ApplicationResult<bool>.Failure(ApplicationError.Conflict(
            "owner_company_profile.in_use",
            "Company profile is used by quotations or invoices and cannot be deleted."));
}

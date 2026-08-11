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
    : ICommandHandler<DeleteOwnerCompanyProfileCommand>
{
    public async ValueTask<CommandResult> Handle(
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

    private async Task<CommandResult> DeleteMediaAsync(
        OwnerCompanyProfileRecord? profile,
        CancellationToken cancellationToken)
    {
        if (profile is null)
        {
            return CommandResult.Succeeded();
        }

        await this.DeleteMediaFileAsync(profile.CoverStorageKey, "cover", cancellationToken);
        await this.DeleteMediaFileAsync(profile.IconStorageKey, "icon", cancellationToken);
        return CommandResult.Succeeded();
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

    private static CommandResult MissingProfile()
    {
        return CommandResult.Failure(
            CommandErrorType.NotFound,
            "Owner company profile was not found.");
    }

    private static CommandResult ProfileInUse()
    {
        return CommandResult.Failure(
            CommandErrorType.Conflict,
            "Company profile is used by quotations or invoices and cannot be deleted.");
    }
}

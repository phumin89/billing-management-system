using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.Abstractions.Results;

namespace BillingManagement.Application.OwnerCompanyProfiles.DeleteOwnerCompanyProfile;

public sealed class DeleteOwnerCompanyProfileHandler(IOwnerCompanyProfileStore store)
    : ICommandHandler<DeleteOwnerCompanyProfileCommand, bool>
{
    public async Task<ApplicationResult<bool>> Handle(
        DeleteOwnerCompanyProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await store.Delete(cancellationToken);

        return result switch
        {
            OwnerCompanyProfileDeleteResult.Deleted => ApplicationResult<bool>.Success(true),
            OwnerCompanyProfileDeleteResult.NotFound => MissingProfile(),
            OwnerCompanyProfileDeleteResult.DependencyConflict => ProfileInUse(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, "Unsupported delete result.")
        };
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

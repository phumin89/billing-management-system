using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;

namespace BillingManagement.Application.OwnerCompanyProfiles.UpdateOwnerCompanyProfile;

public sealed record UpdateOwnerCompanyProfileResult(
    bool Succeeded,
    bool NotFound,
    OwnerCompanyProfileRecord? Profile,
    IReadOnlyDictionary<string, string[]> Errors)
{
    public static UpdateOwnerCompanyProfileResult Success(OwnerCompanyProfileRecord profile) =>
        new(true, false, profile, new Dictionary<string, string[]>());

    public static UpdateOwnerCompanyProfileResult Missing() =>
        new(false, true, null, new Dictionary<string, string[]>());

    public static UpdateOwnerCompanyProfileResult Failed(Dictionary<string, string[]> errors) =>
        new(false, false, null, errors);
}

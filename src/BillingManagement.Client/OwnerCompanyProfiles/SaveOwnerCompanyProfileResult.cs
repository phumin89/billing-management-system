using BillingManagement.Contracts.OwnerCompanyProfiles;

namespace BillingManagement.Client.OwnerCompanyProfiles;

public sealed record SaveOwnerCompanyProfileResult(
    bool Succeeded,
    OwnerCompanyProfileResponse? Profile,
    IReadOnlyDictionary<string, string[]> Errors)
{
    public static SaveOwnerCompanyProfileResult Success(OwnerCompanyProfileResponse profile) =>
        new(true, profile, new Dictionary<string, string[]>());

    public static SaveOwnerCompanyProfileResult Failed(IReadOnlyDictionary<string, string[]> errors) =>
        new(false, null, errors);
}

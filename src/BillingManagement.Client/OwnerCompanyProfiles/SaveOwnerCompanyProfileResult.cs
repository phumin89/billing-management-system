using BillingManagement.Contracts.OwnerCompanyProfiles;

namespace BillingManagement.Client.OwnerCompanyProfiles;

public sealed record SaveOwnerCompanyProfileResult(
    bool Succeeded,
    OwnerCompanyProfileResponse? Profile,
    IReadOnlyDictionary<string, string[]> Errors,
    string? Message = null)
{
    public static SaveOwnerCompanyProfileResult Success(OwnerCompanyProfileResponse profile) =>
        new(true, profile, new Dictionary<string, string[]>());

    public static SaveOwnerCompanyProfileResult Failed(
        IReadOnlyDictionary<string, string[]> errors,
        string? message = null) =>
        new(false, null, errors, message);
}

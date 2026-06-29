using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;

namespace BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;

public sealed record CreateOwnerCompanyProfileResult(
    bool Succeeded,
    OwnerCompanyProfileRecord? Profile,
    IReadOnlyDictionary<string, string[]> Errors)
{
    public static CreateOwnerCompanyProfileResult Success(OwnerCompanyProfileRecord profile) =>
        new(true, profile, new Dictionary<string, string[]>());

    public static CreateOwnerCompanyProfileResult Failed(Dictionary<string, string[]> errors) =>
        new(false, null, errors);
}

namespace BillingManagement.Client.OwnerCompanyProfiles;

public sealed record CompanyProfileCoverResult(bool Succeeded, string? Message)
{
    public static CompanyProfileCoverResult Success() => new(true, null);

    public static CompanyProfileCoverResult Failed(string message) => new(false, message);
}

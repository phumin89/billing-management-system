namespace BillingManagement.Client.OwnerCompanyProfiles;

public sealed record CompanyProfileIconResult(bool Succeeded, string? Message)
{
    public static CompanyProfileIconResult Success() => new(true, null);

    public static CompanyProfileIconResult Failed(string message) => new(false, message);
}

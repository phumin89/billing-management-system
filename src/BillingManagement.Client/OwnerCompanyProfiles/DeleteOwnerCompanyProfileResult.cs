namespace BillingManagement.Client.OwnerCompanyProfiles;

public sealed record DeleteOwnerCompanyProfileResult(bool Succeeded, string? Message = null)
{
    public static DeleteOwnerCompanyProfileResult Success() => new(true);

    public static DeleteOwnerCompanyProfileResult Failed(string message) => new(false, message);
}

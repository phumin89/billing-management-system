namespace BillingManagement.Infrastructure.CompanyMedia;

public sealed class CompanyMediaStorageOptions
{
    public const string SectionName = "CompanyMediaStorage";

    public string RootPath { get; init; } = string.Empty;
}

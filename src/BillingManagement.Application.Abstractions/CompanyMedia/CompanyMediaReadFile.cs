namespace BillingManagement.Application.Abstractions.CompanyMedia;

public sealed record CompanyMediaReadFile(CompanyMediaStorageKey Key, long Length, Stream Content);

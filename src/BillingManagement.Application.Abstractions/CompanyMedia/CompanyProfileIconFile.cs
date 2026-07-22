namespace BillingManagement.Application.Abstractions.CompanyMedia;

public sealed record CompanyProfileIconFile(string ContentType, long Length, Stream Content);

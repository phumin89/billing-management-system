namespace BillingManagement.Application.Abstractions.CompanyMedia;

public sealed record CompanyProfileCoverFile(string ContentType, long Length, Stream Content);

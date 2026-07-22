using BillingManagement.Application.Abstractions.Results;

namespace BillingManagement.Application.Abstractions.CompanyMedia;

public interface ICompanyProfileCoverService
{
    Task<ApplicationResult<CompanyProfileCoverDescriptor>> UploadAsync(
        Stream content,
        CancellationToken cancellationToken = default);

    Task<ApplicationResult<CompanyProfileCoverFile>> OpenReadAsync(
        CancellationToken cancellationToken = default);

    Task<ApplicationResult<bool>> ResetAsync(CancellationToken cancellationToken = default);
}

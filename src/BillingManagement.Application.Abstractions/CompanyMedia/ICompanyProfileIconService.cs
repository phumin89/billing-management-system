using BillingManagement.Application.Abstractions.Results;

namespace BillingManagement.Application.Abstractions.CompanyMedia;

public interface ICompanyProfileIconService
{
    Task<ApplicationResult<CompanyProfileIconDescriptor>> UploadAsync(
        Stream content,
        CancellationToken cancellationToken = default);

    Task<ApplicationResult<CompanyProfileIconFile>> OpenReadAsync(
        CancellationToken cancellationToken = default);

    Task<ApplicationResult<bool>> ResetAsync(CancellationToken cancellationToken = default);
}

namespace BillingManagement.Application.Abstractions.CompanyMedia;

public interface ICompanyMediaStore
{
    Task<CompanyMediaStoredFile> StoreAsync(Stream content, CancellationToken cancellationToken = default);

    Task<CompanyMediaStoredFile> ReplaceAsync(
        CompanyMediaStorageKey key,
        Stream content,
        CancellationToken cancellationToken = default);

    Task<CompanyMediaReadFile?> OpenReadAsync(
        CompanyMediaStorageKey key,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(CompanyMediaStorageKey key, CancellationToken cancellationToken = default);
}

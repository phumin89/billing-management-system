namespace BillingManagement.Application.Abstractions.OwnerCompanyProfiles;

public interface IOwnerCompanyProfileStore
{
    Task<OwnerCompanyProfileRecord?> GetAsync(CancellationToken cancellationToken = default);

    Task<bool> Add(OwnerCompanyProfileRecord profile, CancellationToken cancellationToken = default);

    Task<bool> Update(OwnerCompanyProfileRecord profile, CancellationToken cancellationToken = default);
}

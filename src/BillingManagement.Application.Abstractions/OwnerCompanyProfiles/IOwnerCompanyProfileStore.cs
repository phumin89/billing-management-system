namespace BillingManagement.Application.Abstractions.OwnerCompanyProfiles;

public interface IOwnerCompanyProfileStore
{
    Task<OwnerCompanyProfileRecord?> GetAsync(CancellationToken cancellationToken = default);

    Task Add(OwnerCompanyProfileRecord profile, CancellationToken cancellationToken = default);
}

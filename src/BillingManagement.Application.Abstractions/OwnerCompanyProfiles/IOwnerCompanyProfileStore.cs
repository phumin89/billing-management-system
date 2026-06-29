namespace BillingManagement.Application.Abstractions.OwnerCompanyProfiles;

public interface IOwnerCompanyProfileStore
{
    Task<OwnerCompanyProfileRecord?> Get(CancellationToken cancellationToken = default);

    Task Add(OwnerCompanyProfileRecord profile, CancellationToken cancellationToken = default);
}

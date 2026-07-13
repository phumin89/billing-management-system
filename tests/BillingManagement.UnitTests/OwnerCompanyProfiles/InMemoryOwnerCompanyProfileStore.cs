using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;

namespace BillingManagement.UnitTests.OwnerCompanyProfiles;

internal sealed class InMemoryOwnerCompanyProfileStore : IOwnerCompanyProfileStore
{
    public OwnerCompanyProfileRecord? Profile { get; private set; }

    public Task<OwnerCompanyProfileRecord?> GetAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(this.Profile);

    public Task Add(OwnerCompanyProfileRecord profile, CancellationToken cancellationToken = default)
    {
        this.Profile = profile;
        return Task.CompletedTask;
    }

    public Task<bool> Update(OwnerCompanyProfileRecord profile, CancellationToken cancellationToken = default)
    {
        if (this.Profile is null)
        {
            return Task.FromResult(false);
        }

        this.Profile = profile;
        return Task.FromResult(true);
    }
}

using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.Abstractions.Results;
using BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;

namespace BillingManagement.UnitTests.OwnerCompanyProfiles;

public class CreateOwnerCompanyProfileHandlerTests
{
    [Fact]
    public async Task Handle_creates_owner_company_profile()
    {
        var store = new InMemoryOwnerCompanyProfileStore();
        var handler = new CreateOwnerCompanyProfileHandler(store);

        var result = await handler.Handle(ValidCommand());

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Acme Co", result.Value.CompanyName);
        Assert.Equal("Bangkok", result.Value.City);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.NotNull(store.Profile);
    }

    [Fact]
    public async Task Handle_rejects_second_owner_company_profile()
    {
        var store = new InMemoryOwnerCompanyProfileStore();
        var handler = new CreateOwnerCompanyProfileHandler(store);
        await handler.Handle(ValidCommand());

        var result = await handler.Handle(ValidCommand());

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ApplicationErrorKind.Conflict, result.Error.Kind);
        Assert.Equal("owner_company_profile.already_exists", result.Error.Code);
        Assert.Equal("Owner company profile already exists.", result.Error.Message);
    }

    private static CreateOwnerCompanyProfileCommand ValidCommand() =>
        new(
            CompanyName: "Acme Co",
            AddressLine1: "1 Main Street",
            AddressLine2: null,
            City: "Bangkok",
            PostalCode: "10110",
            Country: "Thailand",
            TaxId: "0105550000000",
            Phone: "+66 2 123 4567",
            Email: "billing@example.com",
            Website: "https://example.com",
            LogoReference: "logos/acme.svg",
            RegistrationNumber: "REG-001");

    private sealed class InMemoryOwnerCompanyProfileStore : IOwnerCompanyProfileStore
    {
        public OwnerCompanyProfileRecord? Profile { get; private set; }

        public Task<OwnerCompanyProfileRecord?> GetAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(this.Profile);

        public Task<bool> Add(OwnerCompanyProfileRecord profile, CancellationToken cancellationToken = default)
        {
            if (this.Profile is not null)
            {
                return Task.FromResult(false);
            }

            this.Profile = profile;
            return Task.FromResult(true);
        }

        public Task<bool> Update(OwnerCompanyProfileRecord profile, CancellationToken cancellationToken = default)
        {
            this.Profile = profile;
            return Task.FromResult(true);
        }

        public Task<OwnerCompanyProfileDeleteResult> Delete(CancellationToken cancellationToken = default)
        {
            if (this.Profile is null)
            {
                return Task.FromResult(OwnerCompanyProfileDeleteResult.NotFound);
            }

            this.Profile = null;
            return Task.FromResult(OwnerCompanyProfileDeleteResult.Deleted);
        }
    }
}

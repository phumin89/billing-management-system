using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
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

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Profile);
        Assert.Equal("Acme Co", result.Profile.CompanyName);
        Assert.Equal("Bangkok", result.Profile.City);
        Assert.NotEqual(Guid.Empty, result.Profile.Id);
        Assert.NotNull(store.Profile);
    }

    [Fact]
    public async Task Handle_rejects_second_owner_company_profile()
    {
        var store = new InMemoryOwnerCompanyProfileStore();
        var handler = new CreateOwnerCompanyProfileHandler(store);
        await handler.Handle(ValidCommand());

        var result = await handler.Handle(ValidCommand());

        Assert.False(result.Succeeded);
        Assert.Contains("Owner company profile already exists.", result.Errors["Profile"]);
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
    }
}

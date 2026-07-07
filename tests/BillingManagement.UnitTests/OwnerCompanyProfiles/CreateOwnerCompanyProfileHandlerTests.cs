using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;

namespace BillingManagement.UnitTests.OwnerCompanyProfiles;

public class CreateOwnerCompanyProfileHandlerTests
{
    [Fact]
    public async Task Handle_rejects_missing_required_fields()
    {
        CreateOwnerCompanyProfileHandler handler = new(new InMemoryOwnerCompanyProfileStore());

        CreateOwnerCompanyProfileResult result = await handler.Handle(new CreateOwnerCompanyProfileCommand(
            CompanyName: " ",
            AddressLine1: "",
            AddressLine2: null,
            City: "",
            PostalCode: "",
            Country: "",
            TaxId: null,
            Phone: null,
            Email: null,
            Website: null,
            LogoReference: null,
            RegistrationNumber: null));

        Assert.False(result.Succeeded);
        Assert.Contains(nameof(CreateOwnerCompanyProfileCommand.CompanyName), result.Errors.Keys);
        Assert.Contains(nameof(CreateOwnerCompanyProfileCommand.AddressLine1), result.Errors.Keys);
        Assert.Contains(nameof(CreateOwnerCompanyProfileCommand.City), result.Errors.Keys);
        Assert.Contains(nameof(CreateOwnerCompanyProfileCommand.PostalCode), result.Errors.Keys);
        Assert.Contains(nameof(CreateOwnerCompanyProfileCommand.Country), result.Errors.Keys);
    }

    [Fact]
    public async Task Handle_creates_owner_company_profile()
    {
        InMemoryOwnerCompanyProfileStore store = new();
        CreateOwnerCompanyProfileHandler handler = new(store);

        CreateOwnerCompanyProfileResult result = await handler.Handle(ValidCommand());

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
        InMemoryOwnerCompanyProfileStore store = new();
        CreateOwnerCompanyProfileHandler handler = new(store);
        await handler.Handle(ValidCommand());

        CreateOwnerCompanyProfileResult result = await handler.Handle(ValidCommand());

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

        public Task Add(OwnerCompanyProfileRecord profile, CancellationToken cancellationToken = default)
        {
            this.Profile = profile;
            return Task.CompletedTask;
        }
    }
}

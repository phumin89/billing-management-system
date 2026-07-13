using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.OwnerCompanyProfiles.UpdateOwnerCompanyProfile;

namespace BillingManagement.UnitTests.OwnerCompanyProfiles;

public sealed class UpdateOwnerCompanyProfileHandlerTests
{
    [Fact]
    public async Task Handle_returns_not_found_when_profile_missing()
    {
        var handler = new UpdateOwnerCompanyProfileHandler(new InMemoryOwnerCompanyProfileStore());

        var result = await handler.Handle(ValidCommand());

        Assert.False(result.Succeeded);
        Assert.True(result.NotFound);
    }

    [Fact]
    public async Task Handle_updates_owner_company_profile()
    {
        var store = new InMemoryOwnerCompanyProfileStore();
        await store.Add(new OwnerCompanyProfileRecord(
            Guid.NewGuid(),
            "Old Co",
            "1 Old Street",
            null,
            "Bangkok",
            "10110",
            "Thailand",
            null,
            null,
            null,
            null,
            null,
            null));

        var handler = new UpdateOwnerCompanyProfileHandler(store);

        var result = await handler.Handle(ValidCommand());

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Profile);
        Assert.Equal(store.Profile!.Id, result.Profile.Id);
        Assert.Equal("Acme Updated", result.Profile.CompanyName);
        Assert.Equal("Chiang Mai", result.Profile.City);
    }

    private static UpdateOwnerCompanyProfileCommand ValidCommand() =>
        new(
            CompanyName: "Acme Updated",
            AddressLine1: "2 New Street",
            AddressLine2: null,
            City: "Chiang Mai",
            PostalCode: "50000",
            Country: "Thailand",
            TaxId: "0105550000000",
            Phone: "+66 2 123 4567",
            Email: "billing@example.com",
            Website: "https://example.com",
            LogoReference: "logos/acme.svg",
            RegistrationNumber: "REG-001");
}

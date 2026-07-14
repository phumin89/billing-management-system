using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.Abstractions.Results;
using BillingManagement.Application.OwnerCompanyProfiles.UpdateOwnerCompanyProfile;

namespace BillingManagement.UnitTests.OwnerCompanyProfiles;

public sealed class UpdateOwnerCompanyProfileHandlerTests
{
    [Fact]
    public async Task Handle_returns_not_found_when_profile_missing()
    {
        var handler = new UpdateOwnerCompanyProfileHandler(new InMemoryOwnerCompanyProfileStore());

        var result = await handler.Handle(ValidCommand());

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ApplicationErrorKind.NotFound, result.Error.Kind);
        Assert.Equal("owner_company_profile.not_found", result.Error.Code);
        Assert.Equal("Owner company profile was not found.", result.Error.Message);
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

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(store.Profile!.Id, result.Value.Id);
        Assert.Equal("Acme Updated", result.Value.CompanyName);
        Assert.Equal("Chiang Mai", result.Value.City);
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

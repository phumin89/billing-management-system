using BillingManagement.Domain;

namespace BillingManagement.UnitTests.OwnerCompanyProfiles;

public sealed class OwnerCompanyProfileTests
{
    [Fact]
    public void Create_generates_unique_non_empty_ids()
    {
        var first = Create();
        var second = Create();

        Assert.NotEqual(Guid.Empty, first.Id);
        Assert.NotEqual(Guid.Empty, second.Id);
        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void Rehydrate_preserves_valid_id_and_rejects_empty_id()
    {
        var id = Guid.NewGuid();

        var profile = Rehydrate(id);

        Assert.Equal(id, profile.Id);
        Assert.Throws<ArgumentException>(() => Rehydrate(Guid.Empty));
    }

    [Fact]
    public void Rehydrate_enforces_the_same_invariants_as_create()
    {
        Assert.Throws<ArgumentException>(() => Rehydrate(Guid.NewGuid(), companyName: " "));
        Assert.Throws<ArgumentException>(() => Rehydrate(Guid.NewGuid(), email: Over(OwnerCompanyProfileConstraints.EmailMaxLength)));
    }

    [Fact]
    public void Domain_does_not_expose_persistence_singleton_key()
    {
        Assert.Null(typeof(OwnerCompanyProfile).GetProperty("SingletonKey"));
        Assert.Null(typeof(OwnerCompanyProfile).GetField("SingletonKeyValue"));
    }

    [Fact]
    public void Create_trims_required_values_and_normalizes_optional_blanks()
    {
        var profile = Create(
            companyName: "  Acme Co  ",
            addressLine1: "  1 Main Street  ",
            addressLine2: "   ",
            cityProvinceState: "  Bangkok  ",
            postalCode: "  10110  ",
            country: "  Thailand  ",
            taxId: "  TAX-1  ",
            phone: " ",
            email: "  billing@example.com  ",
            website: " ",
            logoReference: "  logo.svg  ",
            registrationNumber: " ");

        Assert.Equal("Acme Co", profile.CompanyName);
        Assert.Equal("1 Main Street", profile.AddressLine1);
        Assert.Null(profile.AddressLine2);
        Assert.Equal("Bangkok", profile.CityProvinceState);
        Assert.Equal("10110", profile.PostalCode);
        Assert.Equal("Thailand", profile.Country);
        Assert.Equal("TAX-1", profile.TaxId);
        Assert.Null(profile.Phone);
        Assert.Equal("billing@example.com", profile.Email);
        Assert.Null(profile.Website);
        Assert.Equal("logo.svg", profile.LogoReference);
        Assert.Null(profile.RegistrationNumber);
    }

    [Fact]
    public void Create_rejects_blank_required_values()
    {
        Assert.Throws<ArgumentException>(() => Create(companyName: " "));
        Assert.Throws<ArgumentException>(() => Create(addressLine1: " "));
        Assert.Throws<ArgumentException>(() => Create(cityProvinceState: " "));
        Assert.Throws<ArgumentException>(() => Create(postalCode: " "));
        Assert.Throws<ArgumentException>(() => Create(country: " "));
    }

    [Fact]
    public void Create_rejects_values_over_limits_after_trimming()
    {
        Assert.Throws<ArgumentException>(() => Create(companyName: Over(200)));
        Assert.Throws<ArgumentException>(() => Create(addressLine1: Over(300)));
        Assert.Throws<ArgumentException>(() => Create(addressLine2: Over(300)));
        Assert.Throws<ArgumentException>(() => Create(cityProvinceState: Over(150)));
        Assert.Throws<ArgumentException>(() => Create(postalCode: Over(50)));
        Assert.Throws<ArgumentException>(() => Create(country: Over(100)));
        Assert.Throws<ArgumentException>(() => Create(taxId: Over(100)));
        Assert.Throws<ArgumentException>(() => Create(phone: Over(100)));
        Assert.Throws<ArgumentException>(() => Create(email: Over(254)));
        Assert.Throws<ArgumentException>(() => Create(website: Over(300)));
        Assert.Throws<ArgumentException>(() => Create(logoReference: Over(500)));
        Assert.Throws<ArgumentException>(() => Create(registrationNumber: Over(100)));
    }

    [Fact]
    public void Update_applies_same_normalization_and_limits()
    {
        var profile = Create();

        profile.Update(
            "  Updated Co  ",
            "  2 New Street  ",
            " ",
            "  Chiang Mai  ",
            "  50000  ",
            "  Thailand  ",
            " ",
            "  +66 2 123 4567  ",
            " ",
            "  https://example.com  ",
            " ",
            "  REG-1  ");

        Assert.Equal("Updated Co", profile.CompanyName);
        Assert.Null(profile.AddressLine2);
        Assert.Equal("Chiang Mai", profile.CityProvinceState);
        Assert.Null(profile.TaxId);
        Assert.Equal("+66 2 123 4567", profile.Phone);
        Assert.Null(profile.Email);
        Assert.Equal("https://example.com", profile.Website);
        Assert.Null(profile.LogoReference);
        Assert.Equal("REG-1", profile.RegistrationNumber);

        Assert.Throws<ArgumentException>(() => profile.Update(
            Over(200), "Address", null, "City", "10110", "Thailand",
            null, null, null, null, null, null));
    }

    [Fact]
    public void Update_does_not_partially_mutate_when_a_value_is_invalid()
    {
        var profile = Create();

        Assert.Throws<ArgumentException>(() => profile.Update(
            "Changed Co", "2 New Street", null, "Chiang Mai", "50000", "Thailand",
            null, null, null, null, null, Over(100)));

        Assert.Equal("Acme Co", profile.CompanyName);
        Assert.Equal("1 Main Street", profile.AddressLine1);
        Assert.Equal("Bangkok", profile.CityProvinceState);
        Assert.Null(profile.RegistrationNumber);
    }

    private static OwnerCompanyProfile Create(
        string companyName = "Acme Co",
        string addressLine1 = "1 Main Street",
        string? addressLine2 = null,
        string cityProvinceState = "Bangkok",
        string postalCode = "10110",
        string country = "Thailand",
        string? taxId = null,
        string? phone = null,
        string? email = null,
        string? website = null,
        string? logoReference = null,
        string? registrationNumber = null) =>
        OwnerCompanyProfile.Create(
            companyName, addressLine1, addressLine2,
            cityProvinceState, postalCode, country, taxId, phone, email,
            website, logoReference, registrationNumber);

    private static OwnerCompanyProfile Rehydrate(
        Guid id,
        string companyName = "Acme Co",
        string addressLine1 = "1 Main Street",
        string? addressLine2 = null,
        string cityProvinceState = "Bangkok",
        string postalCode = "10110",
        string country = "Thailand",
        string? taxId = null,
        string? phone = null,
        string? email = null,
        string? website = null,
        string? logoReference = null,
        string? registrationNumber = null) =>
        OwnerCompanyProfile.Rehydrate(
            id, companyName, addressLine1, addressLine2,
            cityProvinceState, postalCode, country, taxId, phone, email,
            website, logoReference, registrationNumber);

    private static string Over(int maximumLength) =>
        $" {new string('x', maximumLength + 1)} ";
}

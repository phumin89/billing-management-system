using BillingManagement.Domain;

namespace BillingManagement.UnitTests.OwnerCompanyProfiles;

public sealed class OwnerCompanyProfileCoverMetadataTests
{
    [Fact]
    public void Profile_exposes_cover_storage_metadata()
    {
        var storageKey = typeof(OwnerCompanyProfile).GetProperty("CoverStorageKey");
        var contentType = typeof(OwnerCompanyProfile).GetProperty("CoverContentType");

        Assert.NotNull(storageKey);
        Assert.Equal(typeof(string), storageKey.PropertyType);
        Assert.NotNull(contentType);
        Assert.Equal(typeof(string), contentType.PropertyType);
    }
}

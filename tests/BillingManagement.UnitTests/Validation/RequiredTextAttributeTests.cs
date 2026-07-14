using BillingManagement.Application.Validation;

namespace BillingManagement.UnitTests.Validation;

public sealed class RequiredTextAttributeTests
{
    private readonly RequiredTextAttribute attribute = new("Name is required.");

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_rejects_missing_text(string? value)
    {
        Assert.False(this.attribute.IsValid(value));
    }

    [Fact]
    public void IsValid_accepts_nonblank_text()
    {
        Assert.True(this.attribute.IsValid(" Acme "));
    }
}

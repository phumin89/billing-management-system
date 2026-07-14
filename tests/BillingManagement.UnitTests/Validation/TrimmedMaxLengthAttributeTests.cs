using System.ComponentModel.DataAnnotations;
using BillingManagement.Application.Validation;

namespace BillingManagement.UnitTests.Validation;

public sealed class TrimmedMaxLengthAttributeTests
{
    private readonly TrimmedMaxLengthAttribute attribute = new(5);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(" 12345 ")]
    public void IsValid_accepts_absent_or_trimmed_values_within_limit(string? value)
    {
        Assert.True(this.attribute.IsValid(value));
    }

    [Fact]
    public void GetValidationResult_rejects_trimmed_value_over_limit_with_current_message()
    {
        var result = this.attribute.GetValidationResult(
            " 123456 ",
            new ValidationContext(new object()) { MemberName = "Name" });

        Assert.NotNull(result);
        Assert.Equal("Must not exceed 5 characters.", result.ErrorMessage);
        Assert.Equal(["Name"], result.MemberNames);
    }
}

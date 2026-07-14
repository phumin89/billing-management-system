using System.ComponentModel.DataAnnotations;
using BillingManagement.Application.Validation;

namespace BillingManagement.UnitTests.Validation;

public sealed class AnnotationCommandValidatorTests
{
    private readonly AnnotationCommandValidator<TestCommand> validator = new();

    [Fact]
    public void Validate_maps_attribute_errors_to_property_keys_in_order()
    {
        var errors = this.validator.Validate(new TestCommand(" ", "123456"));

        Assert.Equal(["Name", "Email"], errors.Keys);
        Assert.Equal(["Name is required."], errors["Name"]);
        Assert.Equal(
            ["Must not exceed 5 characters.", "Email format is invalid."],
            errors["Email"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_accepts_optional_blank_values(string? email)
    {
        var errors = this.validator.Validate(new TestCommand("Acme", email));

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_trims_nonblank_text_before_attribute_validation()
    {
        var errors = this.validator.Validate(new TestCommand("Acme", " a@ "));

        Assert.Equal(["Email format is invalid."], errors["Email"]);
    }

    private sealed record TestCommand(
        [property: RequiredText("Name is required.")]
        [property: TrimmedMaxLength(5)]
        string? Name,
        [property: TrimmedMaxLength(5)]
        [property: EmailAddress(ErrorMessage = "Email format is invalid.")]
        string? Email);
}

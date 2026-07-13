using BillingManagement.Application.Validation;

namespace BillingManagement.UnitTests.Validation;

public sealed class ValidationDictionaryExtensionsTests
{
    [Fact]
    public void AddEmail_rejects_incomplete_address()
    {
        var errors = new Dictionary<string, string[]>();

        errors.AddEmail("Email", "a@");

        Assert.Equal(["Email format is invalid."], errors["Email"]);
    }

    [Fact]
    public void Extensions_accumulate_required_length_and_email_errors()
    {
        var errors = new Dictionary<string, string[]>();
        var value = new string('x', 6);

        errors.AddRequired("Name", " ", "Name is required.");
        errors.AddMaxLength("Name", value, 5);
        errors.AddEmail("Name", value, "Email format is invalid.");

        Assert.Equal(
            ["Name is required.", "Must not exceed 5 characters.", "Email format is invalid."],
            errors["Name"]);
    }

    [Fact]
    public void Extensions_ignore_valid_or_absent_optional_values()
    {
        var errors = new Dictionary<string, string[]>();

        errors.AddRequired("Name", "Acme", "Name is required.");
        errors.AddMaxLength("Optional", null, 5);
        errors.AddEmail("Email", "billing@example.com", "Email format is invalid.");
        errors.AddEmail("OptionalEmail", " ", "Email format is invalid.");

        Assert.Empty(errors);
    }
}

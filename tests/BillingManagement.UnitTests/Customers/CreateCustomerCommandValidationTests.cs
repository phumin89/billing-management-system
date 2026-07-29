using BillingManagement.Application.Customers.CreateCustomer;
using BillingManagement.Application.Validation;
using BillingManagement.Domain;

namespace BillingManagement.UnitTests.Customers;

public sealed class CreateCustomerCommandValidationTests
{
    private readonly AnnotationCommandValidator<CreateCustomerCommand> validator = new();

    [Fact]
    public void Validate_returns_required_and_field_length_errors()
    {
        var command = ValidCommand() with
        {
            CustomerName = " ",
            TaxId = new string('x', CustomerConstraints.TaxIdMaxLength + 1),
            Notes = new string('x', CustomerConstraints.NotesMaxLength + 1)
        };

        var errors = this.validator.Validate(command);

        Assert.Equal(["Customer name is required."], errors[nameof(CreateCustomerCommand.CustomerName)]);
        Assert.Equal(["Must not exceed 100 characters."], errors[nameof(CreateCustomerCommand.TaxId)]);
        Assert.Equal(["Must not exceed 2000 characters."], errors[nameof(CreateCustomerCommand.Notes)]);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData(" a@ ")]
    public void Validate_rejects_invalid_email(string email)
    {
        var errors = this.validator.Validate(ValidCommand() with { Email = email });

        Assert.Equal(["Email format is invalid."], errors[nameof(CreateCustomerCommand.Email)]);
    }

    [Fact]
    public void Validate_accepts_optional_blanks_and_valid_values()
    {
        var errors = this.validator.Validate(ValidCommand() with
        {
            TaxId = " ",
            Email = null,
            Notes = "\t"
        });

        Assert.Empty(errors);
    }

    private static CreateCustomerCommand ValidCommand() =>
        new("Acme", null, "billing@example.com", null, null, null, null, null, null, null, null);
}

using BillingManagement.Application.Abstractions.Results;

namespace BillingManagement.UnitTests.ApplicationResults;

public sealed class ApplicationErrorTests
{
    [Fact]
    public void Validation_preserves_field_and_message_order_and_copies_input()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["CompanyName"] = ["Company name is required.", "Company name is too long."],
            ["Email"] = ["Email format is invalid."]
        };

        var error = ApplicationError.Validation(
            "validation_failed",
            "One or more validation errors occurred.",
            errors);

        errors["CompanyName"][0] = "Changed.";
        errors["Phone"] = ["Changed."];

        Assert.Equal(ApplicationErrorKind.Validation, error.Kind);
        Assert.Equal("validation_failed", error.Code);
        Assert.Equal("One or more validation errors occurred.", error.Message);
        Assert.NotNull(error.ValidationErrors);
        Assert.Equal(["CompanyName", "Email"], error.ValidationErrors.Keys);
        Assert.Equal(
            ["Company name is required.", "Company name is too long."],
            error.ValidationErrors["CompanyName"]);
    }

    [Fact]
    public void Non_validation_errors_do_not_carry_validation_fields()
    {
        ApplicationError[] errors =
        [
            ApplicationError.NotFound("not_found", "Resource was not found."),
            ApplicationError.Conflict("conflict", "Resource already exists."),
            ApplicationError.Forbidden("forbidden", "Action is forbidden."),
            ApplicationError.Failure("failure", "Request could not be completed.")
        ];

        Assert.Equal(
            [
                ApplicationErrorKind.NotFound,
                ApplicationErrorKind.Conflict,
                ApplicationErrorKind.Forbidden,
                ApplicationErrorKind.Failure
            ],
            errors.Select(error => error.Kind));
        Assert.All(errors, error => Assert.Null(error.ValidationErrors));
    }
}

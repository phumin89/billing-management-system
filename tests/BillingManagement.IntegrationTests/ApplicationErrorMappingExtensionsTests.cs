using BillingManagement.Api;
using BillingManagement.Application.Abstractions.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BillingManagement.IntegrationTests;

public sealed class ApplicationErrorMappingExtensionsTests
{
    [Fact]
    public void Non_validation_errors_map_to_expected_problem_details()
    {
        var cases = new[]
        {
            (ApplicationError.NotFound("resource.not_found", "Resource was not found."), 404, "Not Found"),
            (ApplicationError.Conflict("resource.conflict", "Resource already exists."), 409, "Conflict"),
            (ApplicationError.Forbidden("resource.forbidden", "Action is forbidden."), 403, "Forbidden"),
            (ApplicationError.Failure("resource.failure", "Request could not be completed."), 400, "Bad Request")
        };
        var controller = CreateController();

        foreach (var (error, status, title) in cases)
        {
            var result = controller.ToProblemDetails(error);

            var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
            var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
            Assert.Equal(status, objectResult.StatusCode);
            Assert.Equal(status, problem.Status);
            Assert.Equal(title, problem.Title);
            Assert.Equal(error.Message, problem.Detail);
            Assert.Equal(error.Code, problem.Extensions["code"]);
        }
    }

    [Fact]
    public void Validation_maps_ordered_fields_and_messages_to_validation_problem_details()
    {
        var error = ApplicationError.Validation(
            "validation_failed",
            "One or more validation errors occurred.",
            new Dictionary<string, string[]>
            {
                ["CompanyName"] = ["Company name is required.", "Company name is too long."],
                ["Email"] = ["Email format is invalid."]
            });
        var controller = CreateController();

        var result = controller.ToProblemDetails(error);

        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result);
        var problem = Assert.IsType<ValidationProblemDetails>(objectResult.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal(["CompanyName", "Email"], problem.Errors.Keys);
        Assert.Equal(
            ["Company name is required.", "Company name is too long."],
            problem.Errors["CompanyName"]);
        Assert.Equal(["Email format is invalid."], problem.Errors["Email"]);
        Assert.Equal("validation_failed", problem.Extensions["code"]);
    }

    [Fact]
    public void Controller_helpers_preserve_configured_problem_details_metadata()
    {
        var controller = CreateController(services =>
            services.AddProblemDetails(options =>
                options.CustomizeProblemDetails = context =>
                    context.ProblemDetails.Extensions["customized"] = true));
        var validationError = ApplicationError.Validation(
            "validation_failed",
            "Validation failed.",
            new Dictionary<string, string[]> { ["Name"] = ["Name is required."] });
        var conflictError = ApplicationError.Conflict("resource.conflict", "Resource already exists.");

        var validationResult = controller.ToProblemDetails(validationError);
        var conflictResult = controller.ToProblemDetails(conflictError);

        var validationProblem = Assert.IsType<ValidationProblemDetails>(
            Assert.IsAssignableFrom<ObjectResult>(validationResult).Value);
        var conflictProblem = Assert.IsType<ProblemDetails>(
            Assert.IsAssignableFrom<ObjectResult>(conflictResult).Value);
        Assert.Equal(true, validationProblem.Extensions["customized"]);
        Assert.Equal(true, conflictProblem.Extensions["customized"]);
    }

    private static TestController CreateController(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddControllers();
        configure?.Invoke(services);
        var provider = services.BuildServiceProvider();
        return new TestController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = provider }
            }
        };
    }

    private sealed class TestController : ControllerBase;
}

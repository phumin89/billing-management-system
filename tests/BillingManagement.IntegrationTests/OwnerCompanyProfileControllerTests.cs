using System.ComponentModel.DataAnnotations;
using BillingManagement.Api.Controllers;
using BillingManagement.Application;
using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;
using BillingManagement.Application.OwnerCompanyProfiles.GetOwnerCompanyProfile;
using BillingManagement.Application.OwnerCompanyProfiles.UpdateOwnerCompanyProfile;
using BillingManagement.Contracts.OwnerCompanyProfiles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BillingManagement.IntegrationTests;

public sealed class OwnerCompanyProfileControllerTests
{
    [Fact]
    public async Task Create_and_update_reject_the_same_invalid_email()
    {
        const string invalidEmail = "a@";

        var createResponse = await CreateController(new StubStore()).Create(
            ValidCreateRequest(invalidEmail),
            default);
        var updateResponse = await CreateController(new StubStore(ExistingProfile())).Update(
            ValidUpdateRequest(invalidEmail),
            default);

        AssertEmailValidationProblem(createResponse.Result);
        AssertEmailValidationProblem(updateResponse.Result);
    }

    [Fact]
    public async Task Create_and_update_accept_the_same_valid_email()
    {
        const string validEmail = "billing@example.com";

        var createResponse = await CreateController(new StubStore()).Create(
            ValidCreateRequest(validEmail),
            default);
        var updateResponse = await CreateController(new StubStore(ExistingProfile())).Update(
            ValidUpdateRequest(validEmail),
            default);

        Assert.IsType<CreatedAtActionResult>(createResponse.Result);
        Assert.IsType<OkObjectResult>(updateResponse.Result);
    }

    [Fact]
    public void Create_request_leaves_email_validation_to_command_pipeline()
    {
        var request = ValidCreateRequest("a@");
        var errors = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            errors,
            validateAllProperties: true);

        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Fact]
    public async Task Create_dispatches_command_and_preserves_validation_problem_details()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["CompanyName"] = ["Company name is required.", "Must not exceed 200 characters."]
        };
        var dispatcher = new StubCommandDispatcher(
            CommandDispatchResult<CreateOwnerCompanyProfileResult>.Invalid(errors));
        var controller = new OwnerCompanyProfileController(
            dispatcher,
            new GetOwnerCompanyProfileHandler(new StubStore()));

        var response = await controller.Create(new CreateOwnerCompanyProfileRequest(), default);

        Assert.IsType<CreateOwnerCompanyProfileCommand>(dispatcher.Command);
        var objectResult = Assert.IsType<ObjectResult>(response.Result);
        var problem = Assert.IsType<ValidationProblemDetails>(objectResult.Value);
        Assert.Equal(errors["CompanyName"], problem.Errors["CompanyName"]);
    }

    private sealed class StubCommandDispatcher(object response) : ICommandDispatcher
    {
        public object? Command { get; private set; }

        public Task<CommandDispatchResult<TResult>> Send<TCommand, TResult>(
            TCommand command,
            CancellationToken cancellationToken = default)
        {
            this.Command = command;
            return Task.FromResult((CommandDispatchResult<TResult>)response);
        }
    }

    private static OwnerCompanyProfileController CreateController(StubStore store)
    {
        var services = new ServiceCollection();
        services.AddControllers();
        services.AddSingleton<IOwnerCompanyProfileStore>(store);
        services.AddBillingManagementApplication();
        var provider = services.BuildServiceProvider();
        return new OwnerCompanyProfileController(
            provider.GetRequiredService<ICommandDispatcher>(),
            provider.GetRequiredService<GetOwnerCompanyProfileHandler>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = provider }
            }
        };
    }

    private static CreateOwnerCompanyProfileRequest ValidCreateRequest(string email) =>
        new()
        {
            CompanyName = "Acme Co.",
            AddressLine1 = "1 Main Street",
            CityProvinceState = "Bangkok",
            PostalCode = "10110",
            Country = "Thailand",
            Email = email
        };

    private static UpdateOwnerCompanyProfileRequest ValidUpdateRequest(string email) =>
        new()
        {
            CompanyName = "Acme Co.",
            AddressLine1 = "1 Main Street",
            CityProvinceState = "Bangkok",
            PostalCode = "10110",
            Country = "Thailand",
            Email = email
        };

    private static OwnerCompanyProfileRecord ExistingProfile() =>
        new(
            Guid.NewGuid(),
            "Acme Co.",
            "1 Main Street",
            null,
            "Bangkok",
            "10110",
            "Thailand",
            null,
            null,
            "old@example.com",
            null,
            null,
            null);

    private static void AssertEmailValidationProblem(ActionResult? result)
    {
        var objectResult = Assert.IsType<BadRequestObjectResult>(result);
        var problem = Assert.IsType<ValidationProblemDetails>(objectResult.Value);
        Assert.Equal(400, problem.Status);
        Assert.Equal(["Email format is invalid."], problem.Errors["Email"]);
    }

    private sealed class StubStore(OwnerCompanyProfileRecord? profile = null) : IOwnerCompanyProfileStore
    {
        private OwnerCompanyProfileRecord? profile = profile;

        public Task<OwnerCompanyProfileRecord?> GetAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(this.profile);

        public Task<bool> Add(OwnerCompanyProfileRecord profile, CancellationToken cancellationToken = default)
        {
            if (this.profile is not null)
            {
                return Task.FromResult(false);
            }

            this.profile = profile;
            return Task.FromResult(true);
        }

        public Task<bool> Update(OwnerCompanyProfileRecord profile, CancellationToken cancellationToken = default)
        {
            if (this.profile is null)
            {
                return Task.FromResult(false);
            }

            this.profile = profile;
            return Task.FromResult(true);
        }
    }
}

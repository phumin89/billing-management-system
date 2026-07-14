using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using BillingManagement.Api.Controllers;
using BillingManagement.Application;
using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;
using BillingManagement.Application.OwnerCompanyProfiles.GetOwnerCompanyProfile;
using BillingManagement.Application.OwnerCompanyProfiles.UpdateOwnerCompanyProfile;
using BillingManagement.Contracts.OwnerCompanyProfiles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace BillingManagement.IntegrationTests;

public sealed class OwnerCompanyProfileControllerTests
{
    [Fact]
    public async Task Missing_required_create_and_update_use_same_dispatcher_validation_http_response()
    {
        await using var app = await StartHttpApplication(new StubStore(ExistingProfile()));
        using var client = new HttpClient { BaseAddress = GetServerAddress(app) };

        var createResponse = await client.PostAsJsonAsync(
            "/api/owner-company-profile",
            new CreateOwnerCompanyProfileRequest());
        var updateResponse = await client.PutAsJsonAsync(
            "/api/owner-company-profile",
            new UpdateOwnerCompanyProfileRequest());

        Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);
        var createProblem = await createResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        var updateProblem = await updateResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(createProblem);
        Assert.NotNull(updateProblem);
        Assert.Equal(400, createProblem.Status);
        Assert.Equal(400, updateProblem.Status);
        AssertValidationErrors(ExpectedRequiredErrors(), createProblem.Errors);
        AssertValidationErrors(ExpectedRequiredErrors(), updateProblem.Errors);
    }

    [Fact]
    public async Task Create_and_update_preserve_maximum_length_then_email_error_order()
    {
        var email = new string('x', 255);
        await using var app = await StartHttpApplication(new StubStore(ExistingProfile()));
        using var client = new HttpClient { BaseAddress = GetServerAddress(app) };

        var createResponse = await client.PostAsJsonAsync(
            "/api/owner-company-profile",
            ValidCreateRequest(email));
        var updateResponse = await client.PutAsJsonAsync(
            "/api/owner-company-profile",
            ValidUpdateRequest(email));

        Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);
        var createProblem = await createResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        var updateProblem = await updateResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(createProblem);
        Assert.NotNull(updateProblem);
        string[] expected = ["Must not exceed 254 characters.", "Email format is invalid."];
        Assert.Equal(expected, createProblem.Errors["Email"]);
        Assert.Equal(expected, updateProblem.Errors["Email"]);
    }

    [Theory]
    [InlineData("a@")]
    [InlineData(" a@ ")]
    public async Task Create_and_update_reject_the_same_invalid_email(string invalidEmail)
    {
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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_and_update_accept_optional_blank_email(string? email)
    {
        var createResponse = await CreateController(new StubStore()).Create(
            ValidCreateRequest(email),
            default);
        var updateResponse = await CreateController(new StubStore(ExistingProfile())).Update(
            ValidUpdateRequest(email),
            default);

        Assert.IsType<CreatedAtActionResult>(createResponse.Result);
        Assert.IsType<OkObjectResult>(updateResponse.Result);
    }

    [Fact]
    public void Transport_requests_do_not_define_validation_attributes()
    {
        Type[] requestTypes =
        [
            typeof(CreateOwnerCompanyProfileRequest),
            typeof(UpdateOwnerCompanyProfileRequest)
        ];

        foreach (var property in requestTypes.SelectMany(type => type.GetProperties()))
        {
            Assert.Empty(property.GetCustomAttributes(typeof(ValidationAttribute), inherit: true));
        }
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
        var controller = CreateController(dispatcher, new StubStore());

        var response = await controller.Create(new CreateOwnerCompanyProfileRequest(), default);

        Assert.IsType<CreateOwnerCompanyProfileCommand>(dispatcher.Command);
        AssertValidationProblem(response.Result, errors);
    }

    [Fact]
    public async Task Update_dispatches_command_and_preserves_validation_problem_details()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["Email"] = ["Email format is invalid."]
        };
        var dispatcher = new StubCommandDispatcher(
            CommandDispatchResult<UpdateOwnerCompanyProfileResult>.Invalid(errors));
        var controller = CreateController(dispatcher, new StubStore(ExistingProfile()));

        var response = await controller.Update(new UpdateOwnerCompanyProfileRequest(), default);

        Assert.IsType<UpdateOwnerCompanyProfileCommand>(dispatcher.Command);
        AssertValidationProblem(response.Result, errors);
    }

    [Fact]
    public async Task Create_preserves_handler_validation_problem_details()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["Profile"] = ["Owner company profile already exists."]
        };
        var dispatcher = new StubCommandDispatcher(
            CommandDispatchResult<CreateOwnerCompanyProfileResult>.Success(
                CreateOwnerCompanyProfileResult.Failed(errors)));
        var controller = CreateController(dispatcher, new StubStore(ExistingProfile()));

        var response = await controller.Create(ValidCreateRequest("billing@example.com"), default);

        AssertValidationProblem(response.Result, errors);
    }

    [Fact]
    public async Task Update_preserves_handler_validation_problem_details()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["Profile"] = ["Owner company profile could not be updated."]
        };
        var dispatcher = new StubCommandDispatcher(
            CommandDispatchResult<UpdateOwnerCompanyProfileResult>.Success(
                UpdateOwnerCompanyProfileResult.Failed(errors)));
        var controller = CreateController(dispatcher, new StubStore(ExistingProfile()));

        var response = await controller.Update(ValidUpdateRequest("billing@example.com"), default);

        AssertValidationProblem(response.Result, errors);
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

    private static OwnerCompanyProfileController CreateController(
        ICommandDispatcher dispatcher,
        StubStore store)
    {
        var provider = new ServiceCollection()
            .AddControllers()
            .Services
            .BuildServiceProvider();
        return new OwnerCompanyProfileController(
            dispatcher,
            new GetOwnerCompanyProfileHandler(store))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = provider }
            }
        };
    }

    private static async Task<WebApplication> StartHttpApplication(StubStore store)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(OwnerCompanyProfileController).Assembly);
        builder.Services.AddSingleton<IOwnerCompanyProfileStore>(store);
        builder.Services.AddBillingManagementApplication();

        var app = builder.Build();
        app.MapControllers();
        await app.StartAsync();
        return app;
    }

    private static Uri GetServerAddress(WebApplication app)
    {
        var addresses = app.Services
            .GetRequiredService<IServer>()
            .Features
            .Get<IServerAddressesFeature>()!;
        return new Uri(addresses.Addresses.Single());
    }

    private static CreateOwnerCompanyProfileRequest ValidCreateRequest(string? email) =>
        new()
        {
            CompanyName = "Acme Co.",
            AddressLine1 = "1 Main Street",
            CityProvinceState = "Bangkok",
            PostalCode = "10110",
            Country = "Thailand",
            Email = email
        };

    private static UpdateOwnerCompanyProfileRequest ValidUpdateRequest(string? email) =>
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

    private static void AssertValidationProblem(
        ActionResult? result,
        IReadOnlyDictionary<string, string[]> expectedErrors)
    {
        var objectResult = Assert.IsType<BadRequestObjectResult>(result);
        var problem = Assert.IsType<ValidationProblemDetails>(objectResult.Value);
        Assert.Equal(400, problem.Status);
        Assert.Equal(expectedErrors.Keys, problem.Errors.Keys);

        foreach (var error in expectedErrors)
        {
            Assert.Equal(error.Value, problem.Errors[error.Key]);
        }
    }

    private static void AssertValidationErrors(
        IReadOnlyDictionary<string, string[]> expected,
        IDictionary<string, string[]> actual)
    {
        Assert.Equal(expected.Keys.Order(), actual.Keys.Order());
        foreach (var error in expected)
        {
            Assert.Equal(error.Value, actual[error.Key]);
        }
    }

    private static IReadOnlyDictionary<string, string[]> ExpectedRequiredErrors() =>
        new Dictionary<string, string[]>
        {
            ["CompanyName"] = ["Company name is required."],
            ["AddressLine1"] = ["Address line 1 is required."],
            ["City"] = ["City / province / state is required."],
            ["PostalCode"] = ["Postal code is required."],
            ["Country"] = ["Country is required."]
        };

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

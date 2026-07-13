using BillingManagement.Api.Controllers;
using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;
using BillingManagement.Application.OwnerCompanyProfiles.GetOwnerCompanyProfile;
using BillingManagement.Contracts.OwnerCompanyProfiles;
using Microsoft.AspNetCore.Mvc;

namespace BillingManagement.IntegrationTests;

public sealed class OwnerCompanyProfileControllerTests
{
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

    private sealed class StubStore : IOwnerCompanyProfileStore
    {
        public Task<OwnerCompanyProfileRecord?> GetAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<OwnerCompanyProfileRecord?>(null);

        public Task<bool> Add(OwnerCompanyProfileRecord profile, CancellationToken cancellationToken = default) =>
            Task.FromResult(true);

        public Task<bool> Update(OwnerCompanyProfileRecord profile, CancellationToken cancellationToken = default) =>
            Task.FromResult(true);
    }
}

using BillingManagement.Application;
using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.Commands;
using BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;
using BillingManagement.Application.OwnerCompanyProfiles.UpdateOwnerCompanyProfile;
using Microsoft.Extensions.DependencyInjection;

namespace BillingManagement.UnitTests;

public sealed class ApplicationServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBillingManagementApplication_registers_dispatcher_handlers_and_validators_explicitly()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IOwnerCompanyProfileStore, StubStore>();
        services.AddBillingManagementApplication();
        using var provider = services.BuildServiceProvider();

        Assert.IsType<CommandDispatcher>(provider.GetRequiredService<ICommandDispatcher>());
        Assert.IsType<CreateOwnerCompanyProfileHandler>(
            provider.GetRequiredService<ICommandHandler<CreateOwnerCompanyProfileCommand, CreateOwnerCompanyProfileResult>>());
        Assert.IsType<UpdateOwnerCompanyProfileHandler>(
            provider.GetRequiredService<ICommandHandler<UpdateOwnerCompanyProfileCommand, UpdateOwnerCompanyProfileResult>>());
        Assert.IsType<CreateOwnerCompanyProfileValidator>(
            Assert.Single(provider.GetServices<ICommandValidator<CreateOwnerCompanyProfileCommand>>()));
        Assert.IsType<UpdateOwnerCompanyProfileValidator>(
            Assert.Single(provider.GetServices<ICommandValidator<UpdateOwnerCompanyProfileCommand>>()));
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

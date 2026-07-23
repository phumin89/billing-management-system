using BillingManagement.Application;
using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.CompanyMedia;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.Commands;
using BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;
using BillingManagement.Application.OwnerCompanyProfiles.DeleteOwnerCompanyProfile;
using BillingManagement.Application.OwnerCompanyProfiles.UpdateOwnerCompanyProfile;
using BillingManagement.Application.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BillingManagement.UnitTests;

public sealed class ApplicationServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBillingManagementApplication_registers_dispatcher_handlers_and_open_generic_annotation_validator()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IOwnerCompanyProfileStore, StubStore>();
        services.AddSingleton<ICompanyMediaStore, StubMediaStore>();
        services.AddSingleton<ILogger<DeleteOwnerCompanyProfileHandler>>(
            NullLogger<DeleteOwnerCompanyProfileHandler>.Instance);
        services.AddBillingManagementApplication();
        using var provider = services.BuildServiceProvider();

        Assert.IsType<CommandDispatcher>(provider.GetRequiredService<ICommandDispatcher>());
        Assert.IsType<CreateOwnerCompanyProfileHandler>(
            provider.GetRequiredService<ICommandHandler<CreateOwnerCompanyProfileCommand, OwnerCompanyProfileRecord>>());
        Assert.IsType<DeleteOwnerCompanyProfileHandler>(
            provider.GetRequiredService<ICommandHandler<DeleteOwnerCompanyProfileCommand, bool>>());
        Assert.IsType<UpdateOwnerCompanyProfileHandler>(
            provider.GetRequiredService<ICommandHandler<UpdateOwnerCompanyProfileCommand, OwnerCompanyProfileRecord>>());
        Assert.IsType<AnnotationCommandValidator<CreateOwnerCompanyProfileCommand>>(
            Assert.Single(provider.GetServices<ICommandValidator<CreateOwnerCompanyProfileCommand>>()));
        Assert.IsType<AnnotationCommandValidator<UpdateOwnerCompanyProfileCommand>>(
            Assert.Single(provider.GetServices<ICommandValidator<UpdateOwnerCompanyProfileCommand>>()));
        Assert.IsType<AnnotationCommandValidator<TestCommand>>(
            provider.GetRequiredService<ICommandValidator<TestCommand>>());
    }

    private sealed record TestCommand;

    private sealed class StubStore : IOwnerCompanyProfileStore
    {
        public Task<OwnerCompanyProfileRecord?> GetAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<OwnerCompanyProfileRecord?>(null);

        public Task<bool> Add(OwnerCompanyProfileRecord profile, CancellationToken cancellationToken = default) =>
            Task.FromResult(true);

        public Task<bool> Update(OwnerCompanyProfileRecord profile, CancellationToken cancellationToken = default) =>
            Task.FromResult(true);

        public Task<OwnerCompanyProfileDeleteResult> Delete(CancellationToken cancellationToken = default) =>
            Task.FromResult(OwnerCompanyProfileDeleteResult.NotFound);
    }

    private sealed class StubMediaStore : ICompanyMediaStore
    {
        public Task<CompanyMediaStoredFile> StoreAsync(
            Stream content,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<CompanyMediaStoredFile> ReplaceAsync(
            CompanyMediaStorageKey key,
            Stream content,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<CompanyMediaReadFile?> OpenReadAsync(
            CompanyMediaStorageKey key,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<bool> DeleteAsync(
            CompanyMediaStorageKey key,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
    }
}

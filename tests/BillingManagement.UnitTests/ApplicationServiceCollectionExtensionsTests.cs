using BillingManagement.Application;
using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.CompanyMedia;
using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.Abstractions.Queries;
using BillingManagement.Application.Customers.UpdateCustomer;
using BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;
using BillingManagement.Application.OwnerCompanyProfiles.UpdateOwnerCompanyProfile;
using BillingManagement.Application.Validation;
using BillingManagement.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace BillingManagement.UnitTests;

public sealed class ApplicationServiceCollectionExtensionsTests
{
    [Fact]
    public void AddBillingManagementApplication_registers_open_generic_annotation_validator()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IOwnerCompanyProfileStore, StubStore>();
        services.AddSingleton<StubCustomerStore>();
        services.AddSingleton<ICustomerStore>(provider => provider.GetRequiredService<StubCustomerStore>());
        services.AddSingleton<ICustomerQueries>(provider => provider.GetRequiredService<StubCustomerStore>());
        services.AddSingleton<ICompanyMediaStore, StubMediaStore>();
        services.AddBillingManagementApplication();
        using var provider = services.BuildServiceProvider();

        Assert.IsType<AnnotationCommandValidator<CreateOwnerCompanyProfileCommand>>(
            Assert.Single(provider.GetServices<ICommandValidator<CreateOwnerCompanyProfileCommand>>()));
        Assert.IsType<AnnotationCommandValidator<UpdateOwnerCompanyProfileCommand>>(
            Assert.Single(provider.GetServices<ICommandValidator<UpdateOwnerCompanyProfileCommand>>()));
        Assert.IsType<AnnotationCommandValidator<UpdateCustomerCommand>>(
            Assert.Single(provider.GetServices<ICommandValidator<UpdateCustomerCommand>>()));
    }

    private sealed class StubCustomerStore : ICustomerStore, ICustomerQueries
    {
        public Task<CustomerRecord?> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<CustomerRecord?>(null);
        }

        public Task<CustomerPage> Search(
            CustomerSearchCriteria criteria,
            PageRequest page,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new CustomerPage([], page.PageNumber, page.PageSize, 0));
        }

        public Task Add(Customer customer, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<bool> Update(Customer customer, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<bool> Delete(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
    }

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

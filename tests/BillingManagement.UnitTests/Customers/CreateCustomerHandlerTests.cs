using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Customers.CreateCustomer;
using BillingManagement.Domain;

namespace BillingManagement.UnitTests.Customers;

public sealed class CreateCustomerHandlerTests
{
    [Fact]
    public async Task Handle_uses_domain_factory_and_persists_normalized_record()
    {
        var store = new RecordingCustomerStore();
        var handler = new CreateCustomerHandler(store);

        var result = await handler.Handle(new CreateCustomerCommand(
            Guid.NewGuid(),
            " Acme ", " ", " billing@example.com ", "\t", " 1 Main Street ", null,
            " Bangkok ", " ", " Thailand ", " Jane ", "\r\n"));

        Assert.True(result.Success);
        Assert.NotEqual(Guid.Empty, store.Added!.Id);
        Assert.Equal("Acme", store.Added.CustomerName);
        Assert.Null(store.Added.TaxId);
        Assert.Equal("billing@example.com", store.Added.Email);
        Assert.Null(store.Added.Phone);
        Assert.Equal("1 Main Street", store.Added.BillingAddressLine1);
        Assert.Equal("Bangkok", store.Added.CityProvinceState);
        Assert.Null(store.Added.PostalCode);
        Assert.Equal("Thailand", store.Added.Country);
        Assert.Equal("Jane", store.Added.ContactName);
        Assert.Null(store.Added.Notes);
    }

    private sealed class RecordingCustomerStore : ICustomerStore
    {
        public Customer? Added { get; private set; }

        public Task Add(Customer customer, CancellationToken cancellationToken = default)
        {
            this.Added = customer;
            return Task.CompletedTask;
        }

        public Task<bool> Update(Customer customer, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<bool> Delete(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
    }
}

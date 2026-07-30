using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Customers.CreateCustomer;

namespace BillingManagement.UnitTests.Customers;

public sealed class CreateCustomerHandlerTests
{
    [Fact]
    public async Task Handle_uses_domain_factory_and_persists_normalized_record()
    {
        var store = new RecordingCustomerStore();
        var handler = new CreateCustomerHandler(store);

        var result = await handler.Handle(new CreateCustomerCommand(
            " Acme ", " ", " billing@example.com ", "\t", " 1 Main Street ", null,
            " Bangkok ", " ", " Thailand ", " Jane ", "\r\n"));

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value!.Id);
        Assert.Same(store.Added, result.Value);
        Assert.Equal("Acme", result.Value.CustomerName);
        Assert.Null(result.Value.TaxId);
        Assert.Equal("billing@example.com", result.Value.Email);
        Assert.Null(result.Value.Phone);
        Assert.Equal("1 Main Street", result.Value.BillingAddressLine1);
        Assert.Equal("Bangkok", result.Value.CityProvinceState);
        Assert.Null(result.Value.PostalCode);
        Assert.Equal("Thailand", result.Value.Country);
        Assert.Equal("Jane", result.Value.ContactName);
        Assert.Null(result.Value.Notes);
    }

    private sealed class RecordingCustomerStore : ICustomerStore
    {
        public CustomerRecord? Added { get; private set; }

        public Task<IReadOnlyList<CustomerRecord>> List(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<CustomerRecord>>([]);

        public Task Add(CustomerRecord customer, CancellationToken cancellationToken = default)
        {
            this.Added = customer;
            return Task.CompletedTask;
        }

        public Task<bool> Update(CustomerRecord customer, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
    }
}

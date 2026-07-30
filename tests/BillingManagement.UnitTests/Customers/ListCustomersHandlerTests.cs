using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Customers.ListCustomers;

namespace BillingManagement.UnitTests.Customers;

public sealed class ListCustomersHandlerTests
{
    [Fact]
    public async Task Handle_returns_customers_from_store()
    {
        var customers = new[]
        {
            new CustomerRecord(
                Guid.NewGuid(), "Acme", "123", "billing@example.com", "0123",
                "1 Main Street", "Suite 2", "Bangkok", "10110", "Thailand", "Jane", "Notes")
        };
        var store = new RecordingCustomerStore(customers);
        var handler = new ListCustomersHandler(store);

        var result = await handler.Handle(new ListCustomersQuery());

        Assert.Same(customers, result);
        Assert.True(store.ListCalled);
    }

    private sealed class RecordingCustomerStore(IReadOnlyList<CustomerRecord> customers) : ICustomerStore
    {
        public bool ListCalled { get; private set; }

        public Task<IReadOnlyList<CustomerRecord>> List(CancellationToken cancellationToken = default)
        {
            this.ListCalled = true;
            return Task.FromResult(customers);
        }

        public Task Add(CustomerRecord customer, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<bool> Update(CustomerRecord customer, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
    }
}

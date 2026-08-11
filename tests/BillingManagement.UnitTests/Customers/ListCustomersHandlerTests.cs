using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Abstractions.Queries;
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

        Assert.Same(customers, result.Customers);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(100, result.PageSize);
        Assert.Equal(1, result.TotalCount);
        Assert.True(store.SearchCalled);
    }

    private sealed class RecordingCustomerStore(IReadOnlyList<CustomerRecord> customers) : ICustomerQueries
    {
        public bool SearchCalled { get; private set; }

        public Task<CustomerRecord?> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(customers.FirstOrDefault(customer => customer.Id == id));
        }

        public Task<CustomerPage> Search(
            CustomerSearchCriteria criteria,
            PageRequest page,
            CancellationToken cancellationToken = default)
        {
            this.SearchCalled = true;
            return Task.FromResult(new CustomerPage(customers, page.PageNumber, page.PageSize, customers.Count));
        }
    }
}

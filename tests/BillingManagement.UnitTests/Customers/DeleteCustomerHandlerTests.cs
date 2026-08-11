using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Abstractions.Results;
using BillingManagement.Application.Customers.DeleteCustomer;
using BillingManagement.Domain;

namespace BillingManagement.UnitTests.Customers;

public sealed class DeleteCustomerHandlerTests
{
    [Fact]
    public async Task Handle_deletes_selected_customer_and_returns_success()
    {
        var id = Guid.NewGuid();
        var store = new RecordingCustomerStore { DeleteResult = true };
        var handler = new DeleteCustomerHandler(store);

        var result = await handler.Handle(new DeleteCustomerCommand(id));

        Assert.True(result.Success);
        Assert.Equal(id, store.DeletedId);
    }

    [Fact]
    public async Task Handle_returns_not_found_when_customer_is_missing()
    {
        var handler = new DeleteCustomerHandler(new RecordingCustomerStore { DeleteResult = false });

        var result = await handler.Handle(new DeleteCustomerCommand(Guid.NewGuid()));

        Assert.False(result.Success);
        var error = Assert.Single(result.Errors);
        Assert.Equal(CommandErrorType.NotFound, error.Key);
        Assert.Equal(["Customer was not found."], error.Value);
    }

    private sealed class RecordingCustomerStore : ICustomerStore
    {
        public bool DeleteResult { get; init; }
        public Guid? DeletedId { get; private set; }

        public Task<IReadOnlyList<CustomerRecord>> List(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<CustomerRecord>>([]);

        public Task Add(Customer customer, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<bool> Update(Customer customer, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<bool> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            this.DeletedId = id;
            return Task.FromResult(this.DeleteResult);
        }
    }
}

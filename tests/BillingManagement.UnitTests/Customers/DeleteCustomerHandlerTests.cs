using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Abstractions.Results;
using BillingManagement.Application.Customers.DeleteCustomer;

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

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.Equal(id, store.DeletedId);
    }

    [Fact]
    public async Task Handle_returns_not_found_when_customer_is_missing()
    {
        var handler = new DeleteCustomerHandler(new RecordingCustomerStore { DeleteResult = false });

        var result = await handler.Handle(new DeleteCustomerCommand(Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorKind.NotFound, result.Error!.Kind);
        Assert.Equal("customer.not_found", result.Error.Code);
        Assert.Equal("Customer was not found.", result.Error.Message);
    }

    private sealed class RecordingCustomerStore : ICustomerStore
    {
        public bool DeleteResult { get; init; }
        public Guid? DeletedId { get; private set; }

        public Task<IReadOnlyList<CustomerRecord>> List(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<CustomerRecord>>([]);

        public Task Add(CustomerRecord customer, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<bool> Update(CustomerRecord customer, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<bool> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            this.DeletedId = id;
            return Task.FromResult(this.DeleteResult);
        }
    }
}

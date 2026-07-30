using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Abstractions.Results;
using BillingManagement.Application.Customers.UpdateCustomer;

namespace BillingManagement.UnitTests.Customers;

public sealed class UpdateCustomerHandlerTests
{
    [Fact]
    public async Task Handle_returns_not_found_when_customer_missing()
    {
        var handler = new UpdateCustomerHandler(new RecordingCustomerStore { UpdateResult = false });

        var result = await handler.Handle(ValidCommand(Guid.NewGuid()));

        Assert.False(result.IsSuccess);
        Assert.Equal(ApplicationErrorKind.NotFound, result.Error!.Kind);
        Assert.Equal("customer.not_found", result.Error.Code);
        Assert.Equal("Customer was not found.", result.Error.Message);
    }

    [Fact]
    public async Task Handle_preserves_id_normalizes_and_updates_store()
    {
        var id = Guid.NewGuid();
        var store = new RecordingCustomerStore { UpdateResult = true };
        var handler = new UpdateCustomerHandler(store);

        var result = await handler.Handle(ValidCommand(id) with
        {
            CustomerName = " Acme Updated ",
            TaxId = " ",
            Email = " billing@example.com ",
            Phone = "\t"
        });

        Assert.True(result.IsSuccess);
        Assert.Same(store.Updated, result.Value);
        Assert.Equal(id, result.Value!.Id);
        Assert.Equal("Acme Updated", result.Value.CustomerName);
        Assert.Null(result.Value.TaxId);
        Assert.Equal("billing@example.com", result.Value.Email);
        Assert.Null(result.Value.Phone);
    }

    private static UpdateCustomerCommand ValidCommand(Guid id) =>
        new(id, "Acme", null, null, null, null, null, null, null, null, null, null);

    private sealed class RecordingCustomerStore : ICustomerStore
    {
        public Task<IReadOnlyList<CustomerRecord>> List(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<CustomerRecord>>([]);

        public bool UpdateResult { get; init; }
        public CustomerRecord? Updated { get; private set; }

        public Task Add(CustomerRecord customer, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<bool> Update(CustomerRecord customer, CancellationToken cancellationToken = default)
        {
            this.Updated = customer;
            return Task.FromResult(this.UpdateResult);
        }

        public Task<bool> Delete(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
    }
}

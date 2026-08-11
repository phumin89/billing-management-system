using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Abstractions.Results;
using BillingManagement.Application.Customers.UpdateCustomer;
using BillingManagement.Domain;

namespace BillingManagement.UnitTests.Customers;

public sealed class UpdateCustomerHandlerTests
{
    [Fact]
    public async Task Handle_returns_not_found_when_customer_missing()
    {
        var handler = new UpdateCustomerHandler(new RecordingCustomerStore { UpdateResult = false });

        var result = await handler.Handle(ValidCommand(Guid.NewGuid()));

        Assert.False(result.Success);
        var error = Assert.Single(result.Errors);
        Assert.Equal(CommandErrorType.NotFound, error.Key);
        Assert.Equal(["Customer was not found."], error.Value);
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

        Assert.True(result.Success);
        Assert.Equal(id, store.Updated!.Id);
        Assert.Equal("Acme Updated", store.Updated.CustomerName);
        Assert.Null(store.Updated.TaxId);
        Assert.Equal("billing@example.com", store.Updated.Email);
        Assert.Null(store.Updated.Phone);
    }

    private static UpdateCustomerCommand ValidCommand(Guid id) =>
        new(id, "Acme", null, null, null, null, null, null, null, null, null, null);

    private sealed class RecordingCustomerStore : ICustomerStore
    {
        public bool UpdateResult { get; init; }
        public Customer? Updated { get; private set; }

        public Task Add(Customer customer, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<bool> Update(Customer customer, CancellationToken cancellationToken = default)
        {
            this.Updated = customer;
            return Task.FromResult(this.UpdateResult);
        }

        public Task<bool> Delete(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
    }
}

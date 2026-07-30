namespace BillingManagement.Application.Abstractions.Customers;

public interface ICustomerStore
{
    Task<IReadOnlyList<CustomerRecord>> List(CancellationToken cancellationToken = default);

    Task Add(CustomerRecord customer, CancellationToken cancellationToken = default);

    Task<bool> Update(CustomerRecord customer, CancellationToken cancellationToken = default);
}

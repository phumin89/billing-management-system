using BillingManagement.Domain;

namespace BillingManagement.Application.Abstractions.Customers;

public interface ICustomerStore
{
    Task Add(Customer customer, CancellationToken cancellationToken = default);

    Task<bool> Update(Customer customer, CancellationToken cancellationToken = default);

    Task<bool> Delete(Guid id, CancellationToken cancellationToken = default);
}

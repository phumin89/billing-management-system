using BillingManagement.Application.Abstractions.Queries;

namespace BillingManagement.Application.Abstractions.Customers;

public interface ICustomerQueries
{
    Task<CustomerRecord?> GetById(Guid id, CancellationToken cancellationToken = default);

    Task<CustomerPage> Search(
        CustomerSearchCriteria criteria,
        PageRequest page,
        CancellationToken cancellationToken = default);
}

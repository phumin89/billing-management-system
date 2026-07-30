using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Abstractions.Queries;

namespace BillingManagement.Application.Customers.ListCustomers;

public sealed class ListCustomersHandler(ICustomerStore store)
    : IQueryHandler<ListCustomersQuery, IReadOnlyList<CustomerRecord>>
{
    public Task<IReadOnlyList<CustomerRecord>> Handle(
        ListCustomersQuery query,
        CancellationToken cancellationToken = default) =>
        store.List(cancellationToken);
}

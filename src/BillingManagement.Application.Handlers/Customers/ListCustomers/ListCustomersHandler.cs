using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Abstractions.Queries;

namespace BillingManagement.Application.Customers.ListCustomers;

public sealed class ListCustomersHandler(ICustomerQueries queries)
    : IQueryHandler<ListCustomersQuery, ListCustomersResult>
{
    public async ValueTask<ListCustomersResult> Handle(
        ListCustomersQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = await queries.Search(
            new CustomerSearchCriteria(query.SearchText),
            new PageRequest(query.PageNumber, query.PageSize),
            cancellationToken);

        return new ListCustomersResult(page.Items, page.PageNumber, page.PageSize, page.TotalCount);
    }
}

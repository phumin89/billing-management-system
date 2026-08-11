using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Abstractions.Queries;

namespace BillingManagement.Application.Customers.GetCustomer;

public sealed class GetCustomerHandler(ICustomerQueries queries)
    : IQueryHandler<GetCustomerQuery, GetCustomerResult>
{
    public async ValueTask<GetCustomerResult> Handle(
        GetCustomerQuery query,
        CancellationToken cancellationToken = default)
    {
        return new GetCustomerResult(await queries.GetById(query.Id, cancellationToken));
    }
}

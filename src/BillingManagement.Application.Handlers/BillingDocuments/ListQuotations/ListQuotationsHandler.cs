using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.Queries;
using BillingManagement.Application.BillingDocuments.ListQuotations;

namespace BillingManagement.Application.BillingDocuments.ListQuotations;

public sealed class ListQuotationsHandler(IBillingDocumentStore store) : IQueryHandler<ListQuotationsQuery, ListQuotationsResult>
{
    public async ValueTask<ListQuotationsResult> Handle(ListQuotationsQuery query, CancellationToken cancellationToken = default)
    {
        return new ListQuotationsResult(await store.ListQuotations(cancellationToken));
    }
}

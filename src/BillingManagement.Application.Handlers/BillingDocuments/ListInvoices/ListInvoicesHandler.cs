using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.Queries;
using BillingManagement.Application.BillingDocuments.ListInvoices;

namespace BillingManagement.Application.BillingDocuments.ListInvoices;

public sealed class ListInvoicesHandler(IBillingDocumentStore store) : IQueryHandler<ListInvoicesQuery, ListInvoicesResult>
{
    public async ValueTask<ListInvoicesResult> Handle(ListInvoicesQuery query, CancellationToken cancellationToken = default)
    {
        return new ListInvoicesResult(await store.ListInvoices(cancellationToken));
    }
}

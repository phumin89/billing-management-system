using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.Queries;
using BillingManagement.Application.BillingDocuments.ListInvoices;

namespace BillingManagement.Application.BillingDocuments.ListInvoices;

public sealed class ListInvoicesHandler(IBillingDocumentQueries queries) : IQueryHandler<ListInvoicesQuery, ListInvoicesResult>
{
    public async ValueTask<ListInvoicesResult> Handle(ListInvoicesQuery query, CancellationToken cancellationToken = default)
    {
        var page = await queries.SearchInvoices(
            new InvoiceSearchCriteria(query.SearchText, query.Status, query.Today),
            new PageRequest(query.PageNumber, query.PageSize),
            cancellationToken);
        return new ListInvoicesResult(page.Items, page.PageNumber, page.PageSize, page.TotalCount);
    }
}

using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.Queries;
using BillingManagement.Application.BillingDocuments.ListQuotations;

namespace BillingManagement.Application.BillingDocuments.ListQuotations;

public sealed class ListQuotationsHandler(IBillingDocumentQueries queries) : IQueryHandler<ListQuotationsQuery, ListQuotationsResult>
{
    public async ValueTask<ListQuotationsResult> Handle(ListQuotationsQuery query, CancellationToken cancellationToken = default)
    {
        var page = await queries.SearchQuotations(
            new QuotationSearchCriteria(query.SearchText),
            new PageRequest(query.PageNumber, query.PageSize),
            cancellationToken);
        return new ListQuotationsResult(page.Items, page.PageNumber, page.PageSize, page.TotalCount);
    }
}

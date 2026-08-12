using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.Queries;
using BillingManagement.Application.BillingDocuments.GetInvoiceDashboard;

namespace BillingManagement.Application.BillingDocuments.GetInvoiceDashboard;

public sealed class GetInvoiceDashboardHandler(IBillingDocumentQueries queries)
    : IQueryHandler<GetInvoiceDashboardQuery, GetInvoiceDashboardResult>
{
    public async ValueTask<GetInvoiceDashboardResult> Handle(
        GetInvoiceDashboardQuery query,
        CancellationToken cancellationToken = default)
    {
        return new GetInvoiceDashboardResult(
            await queries.GetInvoiceDashboard(query.Today, cancellationToken));
    }
}

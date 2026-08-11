using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.Queries;
using BillingManagement.Application.BillingDocuments.GetInvoice;

namespace BillingManagement.Application.BillingDocuments.GetInvoice;

public sealed class GetInvoiceHandler(IBillingDocumentStore store) : IQueryHandler<GetInvoiceQuery, GetInvoiceResult>
{
    public async ValueTask<GetInvoiceResult> Handle(GetInvoiceQuery query, CancellationToken cancellationToken = default)
    {
        return new GetInvoiceResult(await store.GetInvoice(query.Id, cancellationToken));
    }
}

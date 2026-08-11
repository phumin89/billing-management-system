using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.Queries;
using BillingManagement.Application.BillingDocuments.GetQuotation;

namespace BillingManagement.Application.BillingDocuments.GetQuotation;

public sealed class GetQuotationHandler(IBillingDocumentStore store) : IQueryHandler<GetQuotationQuery, GetQuotationResult>
{
    public async ValueTask<GetQuotationResult> Handle(GetQuotationQuery query, CancellationToken cancellationToken = default)
    {
        return new GetQuotationResult(await store.GetQuotation(query.Id, cancellationToken));
    }
}

using BillingManagement.Application.Abstractions.Queries;

namespace BillingManagement.Application.BillingDocuments.GetQuotation;

public sealed record GetQuotationQuery(Guid Id) : IQuery<GetQuotationResult>;

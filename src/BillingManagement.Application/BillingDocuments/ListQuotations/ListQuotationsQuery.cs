using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.Queries;

namespace BillingManagement.Application.BillingDocuments.ListQuotations;

public sealed record ListQuotationsQuery : IQuery<ListQuotationsResult>;

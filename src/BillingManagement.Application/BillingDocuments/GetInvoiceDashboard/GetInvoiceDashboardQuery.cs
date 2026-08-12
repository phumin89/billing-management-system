using BillingManagement.Application.Abstractions.Queries;

namespace BillingManagement.Application.BillingDocuments.GetInvoiceDashboard;

public sealed record GetInvoiceDashboardQuery(DateOnly Today) : IQuery<GetInvoiceDashboardResult>;

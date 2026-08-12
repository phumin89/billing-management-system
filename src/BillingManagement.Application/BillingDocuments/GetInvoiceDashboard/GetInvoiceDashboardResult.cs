using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.Results;

namespace BillingManagement.Application.BillingDocuments.GetInvoiceDashboard;

public sealed record GetInvoiceDashboardResult(InvoiceDashboardRecord Dashboard) : IQueryResult;

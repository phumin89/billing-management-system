namespace BillingManagement.Contracts.BillingDocuments;

public sealed record InvoiceDashboardResponse(
    IReadOnlyList<InvoiceCurrencyTotalResponse> Outstanding,
    IReadOnlyList<InvoiceCurrencyTotalResponse> PaidThisMonth,
    IReadOnlyList<InvoiceCurrencyTotalResponse> Overdue,
    IReadOnlyList<InvoiceResponse> RecentInvoices);

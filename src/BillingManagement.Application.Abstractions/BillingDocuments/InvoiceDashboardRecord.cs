namespace BillingManagement.Application.Abstractions.BillingDocuments;

public sealed record InvoiceDashboardRecord(
    IReadOnlyList<InvoiceCurrencyTotalRecord> Outstanding,
    IReadOnlyList<InvoiceCurrencyTotalRecord> PaidThisMonth,
    IReadOnlyList<InvoiceCurrencyTotalRecord> Overdue,
    IReadOnlyList<InvoiceRecord> RecentInvoices);

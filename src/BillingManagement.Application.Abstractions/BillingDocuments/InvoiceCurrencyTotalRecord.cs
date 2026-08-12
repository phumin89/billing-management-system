namespace BillingManagement.Application.Abstractions.BillingDocuments;

public sealed record InvoiceCurrencyTotalRecord(
    string Currency,
    int Count,
    decimal Value);

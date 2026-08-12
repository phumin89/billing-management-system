namespace BillingManagement.Contracts.BillingDocuments;

public sealed record InvoiceCurrencyTotalResponse(
    string Currency,
    int Count,
    decimal Value);

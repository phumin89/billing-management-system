namespace BillingManagement.Contracts.BillingDocuments;

public sealed record MarkInvoicePaidRequest(DateOnly PaidDate, decimal AmountPaid);

namespace BillingManagement.Application.Abstractions.BillingDocuments;

public sealed record QuotationRecord(Guid Id, string Number, Guid CustomerId, string CustomerName, string? CustomerAddress, string? CustomerTaxId, DateOnly IssueDate, DateOnly ValidUntil, string Currency, IReadOnlyList<BillingDocumentItemRecord> Items, decimal Subtotal, decimal TaxTotal, decimal Total);

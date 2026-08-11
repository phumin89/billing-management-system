namespace BillingManagement.Contracts.BillingDocuments;

public sealed record QuotationResponse(Guid Id, string Number, Guid CustomerId, string CustomerName, string? CustomerAddress, string? CustomerTaxId, DateOnly IssueDate, DateOnly ValidUntil, string Currency, IReadOnlyList<BillingDocumentItemResponse> Items, decimal Subtotal, decimal TaxTotal, decimal Total);

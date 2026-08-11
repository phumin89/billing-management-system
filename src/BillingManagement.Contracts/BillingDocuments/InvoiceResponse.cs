namespace BillingManagement.Contracts.BillingDocuments;

public sealed record InvoiceResponse(Guid Id, string Number, Guid QuotationId, Guid CustomerId, string CustomerName, string? CustomerAddress, string? CustomerTaxId, DateOnly IssueDate, DateOnly DueDate, string Currency, IReadOnlyList<BillingDocumentItemResponse> Items, decimal Subtotal, decimal TaxTotal, decimal Total);

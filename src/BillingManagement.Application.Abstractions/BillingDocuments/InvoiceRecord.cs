namespace BillingManagement.Application.Abstractions.BillingDocuments;

public sealed record InvoiceRecord(Guid Id, string Number, string SellerCompanyName, string SellerAddress, string? SellerTaxId, string? SellerPhone, string? SellerEmail, string? SellerWebsite, string? SellerRegistrationNumber, Guid QuotationId, Guid CustomerId, string CustomerName, string? CustomerAddress, string? CustomerTaxId, DateOnly IssueDate, DateOnly DueDate, string Currency, IReadOnlyList<BillingDocumentItemRecord> Items, decimal Subtotal, decimal TaxTotal, decimal Total);

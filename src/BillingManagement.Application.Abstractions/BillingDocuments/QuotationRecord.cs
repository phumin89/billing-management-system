namespace BillingManagement.Application.Abstractions.BillingDocuments;

public sealed record QuotationRecord(Guid Id, string Number, string SellerCompanyName, string SellerAddress, string? SellerTaxId, string? SellerPhone, string? SellerEmail, string? SellerWebsite, string? SellerRegistrationNumber, Guid CustomerId, string CustomerName, string? CustomerAddress, string? CustomerTaxId, DateOnly IssueDate, DateOnly ValidUntil, string Currency, IReadOnlyList<BillingDocumentItemRecord> Items, decimal Subtotal, decimal TaxTotal, decimal Total);

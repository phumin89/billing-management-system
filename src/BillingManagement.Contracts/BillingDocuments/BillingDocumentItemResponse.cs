namespace BillingManagement.Contracts.BillingDocuments;

public sealed record BillingDocumentItemResponse(string Description, decimal Quantity, decimal UnitPrice, decimal TaxRate, decimal LineSubtotal, decimal TaxAmount, decimal LineTotal);

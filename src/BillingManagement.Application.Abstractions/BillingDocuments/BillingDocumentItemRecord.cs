namespace BillingManagement.Application.Abstractions.BillingDocuments;

public sealed record BillingDocumentItemRecord(string Description, decimal Quantity, decimal UnitPrice, decimal TaxRate);

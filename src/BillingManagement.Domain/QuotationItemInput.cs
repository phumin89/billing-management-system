namespace BillingManagement.Domain;

public sealed record QuotationItemInput(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate);

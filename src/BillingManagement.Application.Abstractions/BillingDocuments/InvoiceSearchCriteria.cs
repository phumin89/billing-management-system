using BillingManagement.Domain;

namespace BillingManagement.Application.Abstractions.BillingDocuments;

public sealed record InvoiceSearchCriteria(
    string? SearchText = null,
    InvoiceStatus? Status = null,
    DateOnly? Today = null);

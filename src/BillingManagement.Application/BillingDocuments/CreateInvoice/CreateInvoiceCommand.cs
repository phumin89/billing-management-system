using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Validation;

namespace BillingManagement.Application.BillingDocuments.CreateInvoice;

public sealed record CreateInvoiceCommand(
    Guid Id,
    [property: RequiredText("Invoice number is required.")]
    [property: TrimmedMaxLength(50)] string Number,
    Guid QuotationId,
    DateOnly IssueDate,
    DateOnly DueDate) : ICommand;

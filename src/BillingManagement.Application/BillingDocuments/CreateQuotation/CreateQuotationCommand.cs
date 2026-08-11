using System.ComponentModel.DataAnnotations;
using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Validation;

namespace BillingManagement.Application.BillingDocuments.CreateQuotation;

public sealed record CreateQuotationCommand(
    Guid Id,
    [property: RequiredText("Quotation number is required.")]
    [property: TrimmedMaxLength(50)] string Number,
    Guid CustomerId,
    DateOnly IssueDate,
    DateOnly ValidUntil,
    [property: RequiredText("Currency is required.")]
    [property: StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a three-letter code.")] string Currency,
    [property: MinLength(1, ErrorMessage = "At least one line item is required.")] IReadOnlyList<BillingDocumentItemRecord> Items) : ICommand;

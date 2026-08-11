using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.Commands;

namespace BillingManagement.Application.BillingDocuments.CreateQuotation;

public sealed record CreateQuotationCommand(Guid Id, string Number, Guid CustomerId, DateOnly IssueDate, DateOnly ValidUntil, string Currency, IReadOnlyList<BillingDocumentItemRecord> Items) : ICommand;

using BillingManagement.Application.Abstractions.Commands;

namespace BillingManagement.Application.BillingDocuments.CreateInvoice;

public sealed record CreateInvoiceCommand(Guid Id, string Number, Guid QuotationId, DateOnly IssueDate, DateOnly DueDate) : ICommand;

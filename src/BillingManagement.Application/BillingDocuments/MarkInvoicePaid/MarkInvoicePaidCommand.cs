using BillingManagement.Application.Abstractions.Commands;

namespace BillingManagement.Application.BillingDocuments.MarkInvoicePaid;

public sealed record MarkInvoicePaidCommand(Guid InvoiceId, DateOnly PaidDate, decimal AmountPaid) : ICommand;

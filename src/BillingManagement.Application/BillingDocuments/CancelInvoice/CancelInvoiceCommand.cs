using BillingManagement.Application.Abstractions.Commands;

namespace BillingManagement.Application.BillingDocuments.CancelInvoice;

public sealed record CancelInvoiceCommand(Guid InvoiceId) : ICommand;

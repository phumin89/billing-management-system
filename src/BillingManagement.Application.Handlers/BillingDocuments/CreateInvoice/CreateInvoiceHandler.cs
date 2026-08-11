using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.Results;
using BillingManagement.Application.BillingDocuments.CreateInvoice;
using BillingManagement.Domain;

namespace BillingManagement.Application.BillingDocuments.CreateInvoice;

public sealed class CreateInvoiceHandler(IBillingDocumentStore store) : ICommandHandler<CreateInvoiceCommand>
{
    public async ValueTask<CommandResult> Handle(CreateInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        if (await store.InvoiceNumberExists(command.Number.Trim(), cancellationToken))
        {
            return CommandResult.Failure(CommandErrorType.Conflict, "Invoice number already exists.");
        }

        if (await store.InvoiceExistsForQuotation(command.QuotationId, cancellationToken))
        {
            return CommandResult.Failure(CommandErrorType.Conflict, "Quotation already has an invoice.");
        }

        var quotation = await store.GetQuotationEntity(command.QuotationId, cancellationToken);
        if (quotation is null)
        {
            return CommandResult.Failure(CommandErrorType.NotFound, "Quotation was not found.");
        }

        await store.AddInvoice(Invoice.CreateFromQuotation(command.Id, command.Number, quotation, command.IssueDate, command.DueDate), cancellationToken);
        return CommandResult.Succeeded();
    }
}

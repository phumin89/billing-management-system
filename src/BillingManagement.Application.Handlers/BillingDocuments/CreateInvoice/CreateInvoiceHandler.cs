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
        var quotation = await store.GetQuotationEntity(command.QuotationId, cancellationToken);
        if (quotation is null)
        {
            return CommandResult.Failure(CommandErrorType.NotFound, "Quotation was not found.");
        }

        await store.AddInvoice(Invoice.CreateFromQuotation(command.Id, command.Number, quotation, command.IssueDate, command.DueDate), cancellationToken);
        return CommandResult.Succeeded();
    }
}

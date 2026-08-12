using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.Results;
using BillingManagement.Application.BillingDocuments.CancelInvoice;

namespace BillingManagement.Application.BillingDocuments.CancelInvoice;

public sealed class CancelInvoiceHandler(IBillingDocumentStore store) : ICommandHandler<CancelInvoiceCommand>
{
    public async ValueTask<CommandResult> Handle(CancelInvoiceCommand command, CancellationToken cancellationToken = default)
    {
        var invoice = await store.GetInvoiceEntity(command.InvoiceId, cancellationToken);
        if (invoice is null)
        {
            return CommandResult.Failure(CommandErrorType.NotFound, "Invoice was not found.");
        }

        try
        {
            invoice.Cancel();
        }
        catch (InvalidOperationException exception)
        {
            return CommandResult.Failure(CommandErrorType.Conflict, exception.Message);
        }

        await store.SaveInvoice(cancellationToken);
        return CommandResult.Succeeded();
    }
}

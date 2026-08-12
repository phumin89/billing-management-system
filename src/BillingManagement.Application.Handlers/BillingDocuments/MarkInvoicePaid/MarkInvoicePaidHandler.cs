using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.Results;
using BillingManagement.Application.BillingDocuments.MarkInvoicePaid;

namespace BillingManagement.Application.BillingDocuments.MarkInvoicePaid;

public sealed class MarkInvoicePaidHandler(IBillingDocumentStore store) : ICommandHandler<MarkInvoicePaidCommand>
{
    public async ValueTask<CommandResult> Handle(MarkInvoicePaidCommand command, CancellationToken cancellationToken = default)
    {
        var invoice = await store.GetInvoiceEntity(command.InvoiceId, cancellationToken);
        if (invoice is null)
        {
            return CommandResult.Failure(CommandErrorType.NotFound, "Invoice was not found.");
        }

        try
        {
            invoice.MarkPaid(command.PaidDate, command.AmountPaid);
        }
        catch (ArgumentException exception)
        {
            return CommandResult.Failure(CommandErrorType.Validation, exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return CommandResult.Failure(CommandErrorType.Conflict, exception.Message);
        }

        await store.SaveInvoice(cancellationToken);
        return CommandResult.Succeeded();
    }
}

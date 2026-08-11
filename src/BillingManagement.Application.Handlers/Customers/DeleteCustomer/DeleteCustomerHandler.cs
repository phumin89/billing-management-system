using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Abstractions.Results;

namespace BillingManagement.Application.Customers.DeleteCustomer;

public sealed class DeleteCustomerHandler(ICustomerStore store)
    : ICommandHandler<DeleteCustomerCommand>
{
    public async ValueTask<CommandResult> Handle(
        DeleteCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!await store.Delete(command.Id, cancellationToken))
        {
            return CommandResult.Failure(CommandErrorType.NotFound, "Customer was not found.");
        }

        return CommandResult.Succeeded();
    }
}

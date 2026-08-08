using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Abstractions.Results;

namespace BillingManagement.Application.Customers.DeleteCustomer;

public sealed class DeleteCustomerHandler(ICustomerStore store)
    : ICommandHandler<DeleteCustomerCommand, bool>
{
    public async Task<ApplicationResult<bool>> Handle(
        DeleteCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!await store.Delete(command.Id, cancellationToken))
        {
            return ApplicationResult<bool>.Failure(ApplicationError.NotFound(
                "customer.not_found",
                "Customer was not found."));
        }

        return ApplicationResult<bool>.Success(true);
    }
}

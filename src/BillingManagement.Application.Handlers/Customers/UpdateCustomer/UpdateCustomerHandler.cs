using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Abstractions.Results;
using BillingManagement.Domain;

namespace BillingManagement.Application.Customers.UpdateCustomer;

public sealed class UpdateCustomerHandler(ICustomerStore store)
    : ICommandHandler<UpdateCustomerCommand>
{
    public async ValueTask<CommandResult> Handle(
        UpdateCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        var customer = Customer.Rehydrate(
            command.Id,
            command.CustomerName,
            command.TaxId,
            command.Email,
            command.Phone,
            command.BillingAddressLine1,
            command.BillingAddressLine2,
            command.CityProvinceState,
            command.PostalCode,
            command.Country,
            command.ContactName,
            command.Notes);
        if (!await store.Update(customer, cancellationToken))
        {
            return CommandResult.Failure(CommandErrorType.NotFound, "Customer was not found.");
        }

        return CommandResult.Succeeded();
    }
}

using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Abstractions.Results;
using BillingManagement.Domain;

namespace BillingManagement.Application.Customers.CreateCustomer;

public sealed class CreateCustomerHandler(ICustomerStore store)
    : ICommandHandler<CreateCustomerCommand>
{
    public async ValueTask<CommandResult> Handle(
        CreateCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        var customer = Customer.Create(
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
        await store.Add(customer, cancellationToken);
        return CommandResult.Succeeded();
    }
}

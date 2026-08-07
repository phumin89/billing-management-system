using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Abstractions.Results;
using BillingManagement.Domain;

namespace BillingManagement.Application.Customers.UpdateCustomer;

public sealed class UpdateCustomerHandler(ICustomerStore store)
    : ICommandHandler<UpdateCustomerCommand, CustomerRecord>
{
    public async Task<ApplicationResult<CustomerRecord>> Handle(
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
        var record = ToRecord(customer);

        if (!await store.Update(record, cancellationToken))
        {
            return ApplicationResult<CustomerRecord>.Failure(ApplicationError.NotFound(
                "customer.not_found",
                "Customer was not found."));
        }

        return ApplicationResult<CustomerRecord>.Success(record);
    }

    private static CustomerRecord ToRecord(Customer customer) =>
        new(
            customer.Id,
            customer.CustomerName,
            customer.TaxId,
            customer.Email,
            customer.Phone,
            customer.BillingAddressLine1,
            customer.BillingAddressLine2,
            customer.CityProvinceState,
            customer.PostalCode,
            customer.Country,
            customer.ContactName,
            customer.Notes);
}

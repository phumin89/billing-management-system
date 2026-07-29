using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Domain;

namespace BillingManagement.Infrastructure.Customers;

public sealed class CustomerStore(BillingManagementDbContext dbContext) : ICustomerStore
{
    public async Task Add(CustomerRecord customer, CancellationToken cancellationToken = default)
    {
        dbContext.Customers.Add(Customer.Rehydrate(
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
            customer.Notes));
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

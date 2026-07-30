using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace BillingManagement.Infrastructure.Customers;

public sealed class CustomerStore(BillingManagementDbContext dbContext) : ICustomerStore
{
    public async Task<IReadOnlyList<CustomerRecord>> List(CancellationToken cancellationToken = default) =>
        await dbContext.Customers
            .AsNoTracking()
            .OrderBy(customer => customer.CustomerName)
            .ThenBy(customer => customer.Id)
            .Select(customer => new CustomerRecord(
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
                customer.Notes))
            .ToListAsync(cancellationToken);

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

    public async Task<bool> Update(CustomerRecord customer, CancellationToken cancellationToken = default)
    {
        var existingCustomer = await dbContext.Customers
            .SingleOrDefaultAsync(existing => existing.Id == customer.Id, cancellationToken);
        if (existingCustomer is null)
        {
            return false;
        }

        existingCustomer.Update(
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
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Customers
            .Where(customer => customer.Id == id)
            .ExecuteDeleteAsync(cancellationToken) == 1;
}

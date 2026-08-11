using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Abstractions.Queries;
using BillingManagement.Domain;
using Microsoft.EntityFrameworkCore;

namespace BillingManagement.Infrastructure.Customers;

public sealed class CustomerStore(BillingManagementDbContext dbContext) : ICustomerStore, ICustomerQueries
{
    public async Task<CustomerRecord?> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var customer = dbContext.Customers.AsNoTracking()
            .Where(customer => customer.Id == id);
        return await Project(customer).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CustomerPage> Search(
        CustomerSearchCriteria criteria,
        PageRequest page,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = Math.Max(page.PageNumber, 1);
        var pageSize = Math.Clamp(page.PageSize, 1, 100);
        var matchingCustomers = ApplySearch(dbContext.Customers.AsNoTracking(), criteria);
        var totalCount = await matchingCustomers.CountAsync(cancellationToken);
        var orderedCustomers = matchingCustomers
            .OrderBy(customer => customer.CustomerName)
            .ThenBy(customer => customer.Id);
        var items = await Project(orderedCustomers)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new CustomerPage(items, pageNumber, pageSize, totalCount);
    }

    public async Task Add(Customer customer, CancellationToken cancellationToken = default)
    {
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> Update(Customer customer, CancellationToken cancellationToken = default)
    {
        var existingCustomer = await dbContext.Customers
            .FirstOrDefaultAsync(existing => existing.Id == customer.Id, cancellationToken);
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

    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Customers
            .Where(customer => customer.Id == id)
            .ExecuteDeleteAsync(cancellationToken) == 1;
    }

    private static IQueryable<CustomerRecord> Project(IQueryable<Customer> customers)
    {
        return customers.Select(customer => new CustomerRecord(
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
    }

    private static IQueryable<Customer> ApplySearch(
        IQueryable<Customer> customers,
        CustomerSearchCriteria criteria)
    {
        if (string.IsNullOrWhiteSpace(criteria.SearchText))
        {
            return customers;
        }

        var searchText = criteria.SearchText.Trim();
        return customers.Where(customer =>
            customer.CustomerName.Contains(searchText) ||
            (customer.Email != null && customer.Email.Contains(searchText)) ||
            (customer.TaxId != null && customer.TaxId.Contains(searchText)));
    }
}

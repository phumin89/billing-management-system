using BillingManagement.Contracts.Customers;

namespace BillingManagement.Client.Customers;

public sealed class CustomerSessionState
{
    private readonly List<CustomerResponse> customers = [];

    public IReadOnlyList<CustomerResponse> Customers => this.customers;

    public void Add(CustomerResponse customer) => this.customers.Add(customer);

    public void Replace(CustomerResponse customer)
    {
        var index = this.customers.FindIndex(existing => existing.Id == customer.Id);
        if (index < 0)
        {
            throw new InvalidOperationException("Customer is not present in session state.");
        }

        this.customers[index] = customer;
    }
}

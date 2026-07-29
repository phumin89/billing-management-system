using BillingManagement.Contracts.Customers;

namespace BillingManagement.Client.Customers;

public sealed class CustomerSessionState
{
    private readonly List<CustomerResponse> customers = [];

    public IReadOnlyList<CustomerResponse> Customers => this.customers;

    public void Add(CustomerResponse customer) => this.customers.Add(customer);
}

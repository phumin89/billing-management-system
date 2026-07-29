using BillingManagement.Client.Customers;
using BillingManagement.Contracts.Customers;
using Microsoft.AspNetCore.Components;

namespace BillingManagement.Client.Pages.Customers;

public partial class Customers
{
    [Inject]
    private CustomerSessionState CustomerState { get; set; } = default!;

    private IReadOnlyList<CustomerResponse> CustomerList => this.CustomerState.Customers;

    private static string BillingAddress(CustomerResponse customer) =>
        string.Join(", ", new[]
        {
            customer.BillingAddressLine1,
            customer.BillingAddressLine2,
            customer.CityProvinceState,
            customer.PostalCode,
            customer.Country
        }.Where(value => !string.IsNullOrWhiteSpace(value)));
}

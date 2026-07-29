using BillingManagement.Client.Customers;
using BillingManagement.Contracts.Customers;
using Microsoft.AspNetCore.Components;

namespace BillingManagement.Client.Pages.Customers;

public partial class Customers
{
    private CustomerResponse? editingCustomer;
    private CreateCustomerRequest? editForm;

    [Inject]
    private CustomerSessionState CustomerState { get; set; } = default!;

    private IReadOnlyList<CustomerResponse> CustomerList => this.CustomerState.Customers;

    private bool IsEditing => this.editingCustomer is not null;

    protected override void OnInitialized()
    {
        if (this.CustomerList.Count == 0)
        {
            this.CustomerState.Add(SampleCustomer());
        }
    }

    private void BeginEdit(CustomerResponse customer)
    {
        this.editingCustomer = customer;
        this.editForm = new CreateCustomerRequest
        {
            CustomerName = customer.CustomerName,
            TaxId = customer.TaxId,
            Email = customer.Email,
            Phone = customer.Phone,
            BillingAddressLine1 = customer.BillingAddressLine1,
            BillingAddressLine2 = customer.BillingAddressLine2,
            CityProvinceState = customer.CityProvinceState,
            PostalCode = customer.PostalCode,
            Country = customer.Country,
            ContactName = customer.ContactName,
            Notes = customer.Notes
        };
    }

    private void CancelEdit()
    {
        this.editingCustomer = null;
        this.editForm = null;
    }

    private static string BillingAddress(CustomerResponse customer) =>
        string.Join(", ", new[]
        {
            customer.BillingAddressLine1,
            customer.BillingAddressLine2,
            customer.CityProvinceState,
            customer.PostalCode,
            customer.Country
        }.Where(value => !string.IsNullOrWhiteSpace(value)));

    private static CustomerResponse SampleCustomer() =>
        new()
        {
            Id = Guid.Parse("86fb6f33-5327-4d89-ae07-a678b2955970"),
            CustomerName = "Northstar Studio",
            TaxId = "TH-0105560123456",
            Email = "billing@northstar.example",
            Phone = "+66 2 555 0142",
            BillingAddressLine1 = "88 Wireless Road",
            BillingAddressLine2 = "Unit 1204",
            CityProvinceState = "Bangkok",
            PostalCode = "10330",
            Country = "Thailand",
            ContactName = "Maya Chen",
            Notes = "Monthly billing contact"
        };
}

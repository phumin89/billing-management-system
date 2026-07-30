using BillingManagement.Client.Customers;
using BillingManagement.Contracts.Customers;
using Microsoft.AspNetCore.Components;

namespace BillingManagement.Client.Pages.Customers;

public partial class Customers
{
    private static readonly HashSet<string> FieldNames =
    [
        nameof(UpdateCustomerRequest.CustomerName),
        nameof(UpdateCustomerRequest.TaxId),
        nameof(UpdateCustomerRequest.Email),
        nameof(UpdateCustomerRequest.Phone),
        nameof(UpdateCustomerRequest.BillingAddressLine1),
        nameof(UpdateCustomerRequest.BillingAddressLine2),
        nameof(UpdateCustomerRequest.CityProvinceState),
        nameof(UpdateCustomerRequest.PostalCode),
        nameof(UpdateCustomerRequest.Country),
        nameof(UpdateCustomerRequest.ContactName),
        nameof(UpdateCustomerRequest.Notes)
    ];

    private CustomerResponse? editingCustomer;
    private CreateCustomerRequest? editForm;
    private IReadOnlyDictionary<string, string[]> validationErrors = new Dictionary<string, string[]>();
    private string? statusMessage;
    private bool isSubmitting;

    [Inject]
    private CustomerClient Client { get; set; } = default!;

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
        this.validationErrors = new Dictionary<string, string[]>();
        this.statusMessage = null;
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
        this.validationErrors = new Dictionary<string, string[]>();
        this.statusMessage = null;
    }

    private async Task SaveCustomer()
    {
        if (this.isSubmitting)
        {
            return;
        }

        this.validationErrors = new Dictionary<string, string[]>();
        this.statusMessage = null;
        this.isSubmitting = true;
        try
        {
            var result = await this.Client.Update(
                this.editingCustomer!.Id,
                ToUpdateRequest(this.editForm!));
            if (!result.Succeeded)
            {
                this.validationErrors = result.Errors;
                this.statusMessage = result.Message;
                return;
            }

            this.CustomerState.Replace(result.Customer!);
            this.editingCustomer = null;
            this.editForm = null;
        }
        finally
        {
            this.isSubmitting = false;
        }
    }

    private string FieldError(string fieldName) =>
        this.validationErrors.TryGetValue(fieldName, out var errors)
            ? string.Join(" ", errors)
            : string.Empty;

    private string GeneralError()
    {
        var messages = this.validationErrors
            .Where(error => !FieldNames.Contains(error.Key))
            .SelectMany(error => error.Value);

        return string.Join(" ", this.statusMessage is null ? messages : messages.Prepend(this.statusMessage));
    }

    private static UpdateCustomerRequest ToUpdateRequest(CreateCustomerRequest form) =>
        new()
        {
            CustomerName = form.CustomerName,
            TaxId = form.TaxId,
            Email = form.Email,
            Phone = form.Phone,
            BillingAddressLine1 = form.BillingAddressLine1,
            BillingAddressLine2 = form.BillingAddressLine2,
            CityProvinceState = form.CityProvinceState,
            PostalCode = form.PostalCode,
            Country = form.Country,
            ContactName = form.ContactName,
            Notes = form.Notes
        };

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

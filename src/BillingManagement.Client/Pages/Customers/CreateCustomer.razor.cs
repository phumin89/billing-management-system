using BillingManagement.Client.Customers;
using BillingManagement.Contracts.Customers;
using Microsoft.AspNetCore.Components;

namespace BillingManagement.Client.Pages.Customers;

public partial class CreateCustomer
{
    private static readonly HashSet<string> FieldNames =
    [
        nameof(CreateCustomerRequest.CustomerName),
        nameof(CreateCustomerRequest.TaxId),
        nameof(CreateCustomerRequest.Email),
        nameof(CreateCustomerRequest.Phone),
        nameof(CreateCustomerRequest.BillingAddressLine1),
        nameof(CreateCustomerRequest.BillingAddressLine2),
        nameof(CreateCustomerRequest.CityProvinceState),
        nameof(CreateCustomerRequest.PostalCode),
        nameof(CreateCustomerRequest.Country),
        nameof(CreateCustomerRequest.ContactName),
        nameof(CreateCustomerRequest.Notes)
    ];

    [Inject]
    private CustomerClient Client { get; set; } = default!;

    [Inject]
    private CustomerSessionState CustomerState { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private CreateCustomerRequest form = new();
    private IReadOnlyDictionary<string, string[]> validationErrors = new Dictionary<string, string[]>();
    private string? statusMessage;
    private bool isSubmitting;

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
            var result = await this.Client.Create(this.form);
            if (!result.Succeeded)
            {
                this.validationErrors = result.Errors;
                this.statusMessage = result.Message;
                return;
            }

            this.CustomerState.Add(result.Customer!);
            this.form = new CreateCustomerRequest();
            this.Navigation.NavigateTo("/customers");
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
}

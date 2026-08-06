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
    private CustomerResponse? deletingCustomer;
    private CreateCustomerRequest? editForm;
    private IReadOnlyDictionary<string, string[]> validationErrors = new Dictionary<string, string[]>();
    private string? statusMessage;
    private string? loadError;
    private string? deleteMessage;
    private bool isLoading;
    private bool isSubmitting;
    private bool isDeleting;
    private bool deleteMessageIsError;
    private bool showDeleteSnackbar;
    private bool snackbarClosing;

    [Inject]
    private CustomerClient Client { get; set; } = default!;

    [Inject]
    private CustomerSessionState CustomerState { get; set; } = default!;

    private IReadOnlyList<CustomerResponse> CustomerList => this.CustomerState.Customers;

    private bool IsEditing => this.editingCustomer is not null;

    protected override async Task OnInitializedAsync()
    {
        if (this.CustomerState.IsLoaded)
        {
            return;
        }

        await this.LoadCustomers();
    }

    private Task RetryLoad() => this.LoadCustomers();

    private async Task LoadCustomers()
    {
        if (this.isLoading)
        {
            return;
        }

        this.isLoading = true;
        this.loadError = null;
        try
        {
            var result = await this.Client.List();
            if (!result.Succeeded)
            {
                this.loadError = result.Message;
                return;
            }

            this.CustomerState.ReplaceAll(result.Customers);
        }
        finally
        {
            this.isLoading = false;
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

    private void BeginDelete(CustomerResponse customer)
    {
        this.deletingCustomer = customer;
        this.deleteMessage = null;
        this.deleteMessageIsError = false;
        this.snackbarClosing = false;
        this.showDeleteSnackbar = true;
    }

    private string DeleteSnackbarClass => this.snackbarClosing
        ? "customer-delete-snackbar is-closing"
        : "customer-delete-snackbar";

    private string DeleteMessageClass => this.deleteMessageIsError
        ? "customer-delete-message is-error"
        : "customer-delete-message";

    private async Task CloseDeleteSnackbar() => await this.DismissDeleteSnackbar();

    private async Task ConfirmDelete()
    {
        if (this.isDeleting || this.deletingCustomer is null)
        {
            return;
        }

        var customerId = this.deletingCustomer.Id;
        this.deleteMessage = null;
        this.isDeleting = true;
        try
        {
            var result = await this.Client.Delete(customerId);
            if (result.ShouldRemoveCustomer)
            {
                this.CustomerState.Remove(customerId);
            }

            this.deleteMessage = result.Message;
            this.deleteMessageIsError = !result.ShouldRemoveCustomer;
            await this.DismissDeleteSnackbar();
        }
        finally
        {
            this.isDeleting = false;
        }
    }

    private async Task DismissDeleteSnackbar()
    {
        if (this.snackbarClosing)
        {
            return;
        }

        this.snackbarClosing = true;
        await Task.Delay(220);
        this.showDeleteSnackbar = false;
        this.deletingCustomer = null;
        this.snackbarClosing = false;
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

}

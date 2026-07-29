using BillingManagement.Contracts.Customers;
using Microsoft.AspNetCore.Components;

namespace BillingManagement.Client.Components.Customers;

public partial class CustomerFormFields
{
    [Parameter]
    [EditorRequired]
    public CreateCustomerRequest Form { get; set; } = default!;

    [Parameter]
    public Func<string, string>? FieldError { get; set; }

    private string ErrorFor(string fieldName) => this.FieldError?.Invoke(fieldName) ?? string.Empty;
}

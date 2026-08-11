using BillingManagement.Client.BillingDocuments;
using BillingManagement.Client.Customers;
using BillingManagement.Contracts.BillingDocuments;
using BillingManagement.Contracts.Customers;
using Microsoft.AspNetCore.Components;

namespace BillingManagement.Client.Pages.Quotations;

public partial class CreateQuotation
{
    [Inject] private BillingDocumentClient Documents { get; set; } = default!;
    [Inject] private CustomerClient Customers { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    private CreateQuotationRequest form = NewForm();
    private IReadOnlyList<CustomerResponse> customers = [];
    private string? error;
    private bool isSaving;

    protected override async Task OnInitializedAsync()
    {
        var result = await this.Customers.List();
        this.customers = result.Customers;
        this.error = result.Succeeded ? null : result.Message;
    }

    private void AddLine() { this.form.Items.Add(new BillingDocumentItemRequest { Quantity = 1, TaxRate = 7 }); }
    private void RemoveLine(int index) { this.form.Items.RemoveAt(index); }

    private async Task Save()
    {
        this.isSaving = true;
        try
        {
            var quotation = await this.Documents.CreateQuotation(this.form);
            this.Navigation.NavigateTo($"/quotations/{quotation.Id}");
        }
        catch (HttpRequestException) { this.error = "Could not create quotation. Check all fields and try again."; }
        finally { this.isSaving = false; }
    }

    private static CreateQuotationRequest NewForm()
    {
        return new CreateQuotationRequest { Number = $"Q-{DateTime.Today:yyyyMMdd}", IssueDate = DateOnly.FromDateTime(DateTime.Today), ValidUntil = DateOnly.FromDateTime(DateTime.Today.AddDays(30)), Currency = "THB", Items = [new BillingDocumentItemRequest { Quantity = 1, TaxRate = 7 }] };
    }
}

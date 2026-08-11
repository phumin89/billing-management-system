using BillingManagement.Client.BillingDocuments;
using BillingManagement.Contracts.BillingDocuments;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BillingManagement.Client.Pages.Quotations;

public partial class QuotationDetail
{
    [Parameter] public Guid Id { get; set; }
    [Inject] private BillingDocumentClient Client { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IJSRuntime JavaScript { get; set; } = default!;
    private QuotationResponse? item;
    private string invoiceNumber = $"INV-{DateTime.Today:yyyyMMdd}";
    private DateOnly invoiceDate = DateOnly.FromDateTime(DateTime.Today);
    private DateOnly dueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30));
    private string? error;
    private bool isCreating;

    protected override async Task OnParametersSetAsync()
    {
        try { this.item = await this.Client.GetQuotation(this.Id); }
        catch (HttpRequestException) { this.error = "Could not load quotation."; }
    }

    private async Task CreateInvoice()
    {
        this.isCreating = true;
        try
        {
            var invoice = await this.Client.CreateInvoice(new CreateInvoiceRequest { Number = this.invoiceNumber, QuotationId = this.Id, IssueDate = this.invoiceDate, DueDate = this.dueDate });
            this.Navigation.NavigateTo($"/invoices/{invoice.Id}");
        }
        catch (BillingDocumentClientException exception) { this.error = exception.Message; }
        catch (HttpRequestException) { this.error = "Could not create invoice. Check the API connection and try again."; }
        finally { this.isCreating = false; }
    }

    private async Task Print()
    {
        await this.JavaScript.InvokeVoidAsync("print");
    }

    private string SellerContacts()
    {
        return string.Join(" · ", new[] { this.item?.SellerPhone, this.item?.SellerEmail, this.item?.SellerWebsite }.Where(value => !string.IsNullOrWhiteSpace(value)));
    }
}

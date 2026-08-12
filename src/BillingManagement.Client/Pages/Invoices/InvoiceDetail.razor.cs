using BillingManagement.Client.BillingDocuments;
using BillingManagement.Contracts.BillingDocuments;
using Microsoft.AspNetCore.Components;

namespace BillingManagement.Client.Pages.Invoices;

public partial class InvoiceDetail
{
    [Parameter] public Guid Id { get; set; }
    [Inject] private BillingDocumentClient Client { get; set; } = default!;
    private InvoiceResponse? item;
    private string? error;
    private bool isUpdating;
    private string? actionError;

    protected override async Task OnParametersSetAsync()
    {
        try { this.item = await this.Client.GetInvoice(this.Id); }
        catch (HttpRequestException) { this.error = "Could not load invoice."; }
    }

    private string SellerContacts()
    {
        return string.Join(" · ", new[] { this.item?.SellerPhone, this.item?.SellerEmail, this.item?.SellerWebsite }.Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    private async Task MarkPaid()
    {
        if (this.item is null || this.isUpdating) return;
        await this.UpdateStatus(async () => await this.Client.MarkInvoicePaid(this.Id, new(DateOnly.FromDateTime(DateTime.Today), this.item.Total)));
    }

    private async Task CancelInvoice()
    {
        if (this.item is null || this.isUpdating) return;
        await this.UpdateStatus(async () => await this.Client.CancelInvoice(this.Id));
    }

    private async Task UpdateStatus(Func<Task> operation)
    {
        this.isUpdating = true;
        this.actionError = null;
        try
        {
            await operation();
            this.item = await this.Client.GetInvoice(this.Id);
        }
        catch (BillingDocumentClientException exception)
        {
            this.actionError = exception.Message;
        }
        finally
        {
            this.isUpdating = false;
        }
    }
}

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

    protected override async Task OnParametersSetAsync()
    {
        try { this.item = await this.Client.GetInvoice(this.Id); }
        catch (HttpRequestException) { this.error = "Could not load invoice."; }
    }

    private string SellerContacts()
    {
        return string.Join(" · ", new[] { this.item?.SellerPhone, this.item?.SellerEmail, this.item?.SellerWebsite }.Where(value => !string.IsNullOrWhiteSpace(value)));
    }
}

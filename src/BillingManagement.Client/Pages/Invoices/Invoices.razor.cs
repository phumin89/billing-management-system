using BillingManagement.Client.BillingDocuments;
using BillingManagement.Contracts.BillingDocuments;
using Microsoft.AspNetCore.Components;

namespace BillingManagement.Client.Pages.Invoices;

public partial class Invoices
{
    [Inject] private BillingDocumentClient Client { get; set; } = default!;
    private IReadOnlyList<InvoiceResponse> items = [];
    private bool isLoading = true;
    private string? error;

    protected override async Task OnInitializedAsync()
    {
        try { this.items = await this.Client.ListInvoices(); }
        catch (HttpRequestException) { this.error = "Could not load invoices."; }
        finally { this.isLoading = false; }
    }
}

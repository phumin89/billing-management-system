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
    private string searchText = string.Empty;
    private string statusFilter = "All";
    private IEnumerable<InvoiceResponse> FilteredItems => this.items.Where(item =>
        (this.statusFilter == "All" || item.Status == this.statusFilter) &&
        (string.IsNullOrWhiteSpace(this.searchText) || item.Number.Contains(this.searchText, StringComparison.OrdinalIgnoreCase) || item.CustomerName.Contains(this.searchText, StringComparison.OrdinalIgnoreCase)));

    protected override async Task OnInitializedAsync()
    {
        try { this.items = await this.Client.ListInvoices(); }
        catch (HttpRequestException) { this.error = "Could not load invoices."; }
        finally { this.isLoading = false; }
    }
}

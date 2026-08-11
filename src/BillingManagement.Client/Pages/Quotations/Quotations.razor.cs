using BillingManagement.Client.BillingDocuments;
using BillingManagement.Contracts.BillingDocuments;
using Microsoft.AspNetCore.Components;

namespace BillingManagement.Client.Pages.Quotations;

public partial class Quotations
{
    [Inject] private BillingDocumentClient Client { get; set; } = default!;
    private IReadOnlyList<QuotationResponse> items = [];
    private bool isLoading = true;
    private string? error;

    protected override async Task OnInitializedAsync()
    {
        try { this.items = await this.Client.ListQuotations(); }
        catch (HttpRequestException) { this.error = "Could not load quotations."; }
        finally { this.isLoading = false; }
    }
}

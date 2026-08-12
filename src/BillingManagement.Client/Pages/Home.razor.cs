using BillingManagement.Client.BillingDocuments;
using BillingManagement.Contracts.BillingDocuments;
using Microsoft.AspNetCore.Components;

namespace BillingManagement.Client.Pages;

public partial class Home
{
    [Inject] private BillingDocumentClient BillingDocuments { get; set; } = default!;
    private InvoiceDashboardResponse summary = new([], [], [], []);
    private bool isLoading = true;
    private string? error;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            this.summary = await this.BillingDocuments.GetInvoiceDashboard();
        }
        catch (HttpRequestException)
        {
            this.error = "Could not load invoice activity.";
        }
        finally
        {
            this.isLoading = false;
        }
    }

    private string Money(IReadOnlyList<InvoiceCurrencyTotalResponse> totals)
    {
        return totals.Count == 0
            ? "0.00"
            : string.Join(" · ", totals.Select(item => $"{item.Value:N2} {item.Currency}"));
    }

    private int Count(IReadOnlyList<InvoiceCurrencyTotalResponse> totals)
    {
        return totals.Sum(item => item.Count);
    }
}

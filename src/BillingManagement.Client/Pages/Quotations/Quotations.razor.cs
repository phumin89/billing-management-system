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
    private string searchText = string.Empty;
    private int pageNumber = 1;
    private int TotalCount { get; set; }
    private const int PageSize = 20;
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(this.TotalCount / (double)PageSize));

    protected override async Task OnInitializedAsync()
    {
        await this.LoadPage();
    }

    private async Task LoadPage()
    {
        this.isLoading = true;
        this.error = null;
        try
        {
            var result = await this.Client.ListQuotations(this.searchText, this.pageNumber, PageSize);
            this.items = result.Items;
            this.pageNumber = result.PageNumber;
            this.TotalCount = result.TotalCount;
        }
        catch (HttpRequestException) { this.error = "Could not load quotations."; }
        finally { this.isLoading = false; }
    }

    private async Task ApplyFilters()
    {
        this.pageNumber = 1;
        await this.LoadPage();
    }

    private async Task PreviousPage()
    {
        this.pageNumber--;
        await this.LoadPage();
    }

    private async Task NextPage()
    {
        this.pageNumber++;
        await this.LoadPage();
    }
}

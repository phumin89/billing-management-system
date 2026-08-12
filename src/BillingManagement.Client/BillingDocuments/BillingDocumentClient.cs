using System.Net.Http.Json;
using BillingManagement.Contracts.BillingDocuments;

namespace BillingManagement.Client.BillingDocuments;

public sealed class BillingDocumentClient(HttpClient httpClient)
{
    public async Task<BillingDocumentPage<QuotationResponse>> ListQuotations(
        string? searchText = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var uri = BuildListUri("api/quotations", searchText, null, pageNumber, pageSize);
        return await this.GetPage<QuotationResponse>(uri, cancellationToken);
    }

    public Task<QuotationResponse?> GetQuotation(Guid id, CancellationToken cancellationToken = default)
    {
        return httpClient.GetFromJsonAsync<QuotationResponse>($"api/quotations/{id}", cancellationToken);
    }

    public async Task<QuotationResponse> CreateQuotation(CreateQuotationRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/quotations", request, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<QuotationResponse>(cancellationToken))!;
    }

    public async Task<BillingDocumentPage<InvoiceResponse>> ListInvoices(
        string? searchText = null,
        string? status = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var uri = BuildListUri("api/invoices", searchText, status, pageNumber, pageSize);
        return await this.GetPage<InvoiceResponse>(uri, cancellationToken);
    }

    public Task<InvoiceResponse?> GetInvoice(Guid id, CancellationToken cancellationToken = default)
    {
        return httpClient.GetFromJsonAsync<InvoiceResponse>($"api/invoices/{id}", cancellationToken);
    }

    public async Task<InvoiceResponse> CreateInvoice(CreateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/invoices", request, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
        return (await response.Content.ReadFromJsonAsync<InvoiceResponse>(cancellationToken))!;
    }

    public async Task MarkInvoicePaid(Guid id, MarkInvoicePaidRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"api/invoices/{id}/mark-paid", request, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
    }

    public async Task CancelInvoice(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync($"api/invoices/{id}/cancel", null, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
    }

    private static async Task EnsureSuccess(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>(cancellationToken);
        var validationMessage = problem?.Errors?.Values.SelectMany(messages => messages).FirstOrDefault();
        throw new BillingDocumentClientException(validationMessage ?? problem?.Detail ?? "The billing document request failed.");
    }

    private async Task<BillingDocumentPage<T>> GetPage<T>(
        string uri,
        CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync(uri, cancellationToken);
        await EnsureSuccess(response, cancellationToken);
        var items = await response.Content.ReadFromJsonAsync<List<T>>(cancellationToken) ?? [];
        var pageNumber = ReadHeader(response, "X-Page-Number", 1);
        var pageSize = ReadHeader(response, "X-Page-Size", 20);
        var totalCount = ReadHeader(response, "X-Total-Count", items.Count);
        return new BillingDocumentPage<T>(items, pageNumber, pageSize, totalCount);
    }

    private static string BuildListUri(
        string path,
        string? searchText,
        string? status,
        int pageNumber,
        int pageSize)
    {
        var parameters = new List<string>
        {
            $"pageNumber={pageNumber}",
            $"pageSize={pageSize}"
        };
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            parameters.Add($"searchText={Uri.EscapeDataString(searchText.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(status) && status != "All")
        {
            parameters.Add($"status={Uri.EscapeDataString(status)}");
        }

        return $"{path}?{string.Join('&', parameters)}";
    }

    private static int ReadHeader(HttpResponseMessage response, string name, int fallback)
    {
        if (!response.Headers.TryGetValues(name, out var values))
        {
            return fallback;
        }

        return int.TryParse(values.FirstOrDefault(), out var value) ? value : fallback;
    }

    private sealed class ProblemResponse
    {
        public string? Detail { get; init; }
        public IReadOnlyDictionary<string, string[]>? Errors { get; init; }
    }
}

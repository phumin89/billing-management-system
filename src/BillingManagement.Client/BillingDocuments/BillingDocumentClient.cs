using System.Net.Http.Json;
using BillingManagement.Contracts.BillingDocuments;

namespace BillingManagement.Client.BillingDocuments;

public sealed class BillingDocumentClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<QuotationResponse>> ListQuotations(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<QuotationResponse>>("api/quotations", cancellationToken) ?? [];
    }

    public Task<QuotationResponse?> GetQuotation(Guid id, CancellationToken cancellationToken = default)
    {
        return httpClient.GetFromJsonAsync<QuotationResponse>($"api/quotations/{id}", cancellationToken);
    }

    public async Task<QuotationResponse> CreateQuotation(CreateQuotationRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/quotations", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<QuotationResponse>(cancellationToken))!;
    }

    public async Task<IReadOnlyList<InvoiceResponse>> ListInvoices(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<InvoiceResponse>>("api/invoices", cancellationToken) ?? [];
    }

    public Task<InvoiceResponse?> GetInvoice(Guid id, CancellationToken cancellationToken = default)
    {
        return httpClient.GetFromJsonAsync<InvoiceResponse>($"api/invoices/{id}", cancellationToken);
    }

    public async Task<InvoiceResponse> CreateInvoice(CreateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/invoices", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<InvoiceResponse>(cancellationToken))!;
    }
}

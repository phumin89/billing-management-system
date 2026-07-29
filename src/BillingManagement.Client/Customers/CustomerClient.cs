using System.Net;
using System.Net.Http.Json;
using BillingManagement.Contracts.Customers;

namespace BillingManagement.Client.Customers;

public sealed class CustomerClient(HttpClient httpClient)
{
    public async Task<CreateCustomerResult> Create(
        CreateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync("api/customers", request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return CreateCustomerResult.Failed(
                new Dictionary<string, string[]>(),
                "Could not save customer. Check the API connection and try again.");
        }

        if (response.IsSuccessStatusCode)
        {
            var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>(cancellationToken);
            return CreateCustomerResult.Success(customer!);
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>(cancellationToken);
            return CreateCustomerResult.Failed(problem?.Errors ?? []);
        }

        return CreateCustomerResult.Failed(
            new Dictionary<string, string[]>(),
            "Could not save customer. Try again.");
    }

    private sealed class ValidationProblemResponse
    {
        public Dictionary<string, string[]> Errors { get; set; } = [];
    }
}

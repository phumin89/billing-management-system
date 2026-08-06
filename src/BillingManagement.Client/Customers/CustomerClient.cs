using System.Net;
using System.Net.Http.Json;
using BillingManagement.Contracts.Customers;

namespace BillingManagement.Client.Customers;

public sealed class CustomerClient(HttpClient httpClient)
{
    public async Task<ListCustomersResult> List(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync("api/customers", cancellationToken);
        }
        catch (HttpRequestException)
        {
            return ListCustomersResult.Failed(
                "Could not load customers. Check the API connection and try again.");
        }

        if (!response.IsSuccessStatusCode)
        {
            return ListCustomersResult.Failed("Could not load customers. Try again.");
        }

        var customers = await response.Content.ReadFromJsonAsync<List<CustomerResponse>>(cancellationToken) ?? [];
        return ListCustomersResult.Success(customers);
    }

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

    public async Task<UpdateCustomerResult> Update(
        Guid id,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PutAsJsonAsync($"api/customers/{id}", request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return UpdateCustomerResult.Failed(
                new Dictionary<string, string[]>(),
                "Could not update customer. Check the API connection and try again.");
        }

        if (response.IsSuccessStatusCode)
        {
            var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>(cancellationToken);
            return UpdateCustomerResult.Success(customer!);
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemResponse>(cancellationToken);
            return UpdateCustomerResult.Failed(problem?.Errors ?? []);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return UpdateCustomerResult.Failed(
                new Dictionary<string, string[]>(),
                "Customer was not found. Refresh and try again.");
        }

        return UpdateCustomerResult.Failed(
            new Dictionary<string, string[]>(),
            "Could not update customer. Try again.");
    }

    public async Task<DeleteCustomerResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.DeleteAsync($"api/customers/{id}", cancellationToken);
        }
        catch (HttpRequestException)
        {
            return DeleteCustomerResult.Failed(
                "Could not delete customer. Check the API connection and try again.");
        }

        if (response.StatusCode is HttpStatusCode.NoContent)
        {
            return DeleteCustomerResult.Removed();
        }

        if (response.StatusCode is HttpStatusCode.NotFound)
        {
            return DeleteCustomerResult.Removed(
                "Customer was not found and was removed from this list.");
        }

        if (response.StatusCode is HttpStatusCode.Conflict)
        {
            return DeleteCustomerResult.Failed(
                "Customer is used by quotations or invoices and cannot be deleted.");
        }

        return DeleteCustomerResult.Failed("Could not delete customer. Try again.");
    }

    private sealed class ValidationProblemResponse
    {
        public Dictionary<string, string[]> Errors { get; set; } = [];
    }
}

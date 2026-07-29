using System.Net;
using System.Net.Http.Json;
using BillingManagement.Client.Customers;
using BillingManagement.Contracts.Customers;

namespace BillingManagement.UnitTests.Customers;

public sealed class CustomerClientTests
{
    [Fact]
    public async Task Create_posts_request_and_returns_customer()
    {
        HttpMethod? method = null;
        string? requestUri = null;
        CreateCustomerRequest? sent = null;
        var customer = new CustomerResponse { Id = Guid.NewGuid(), CustomerName = "Acme" };
        var client = CreateClient(async request =>
        {
            method = request.Method;
            requestUri = request.RequestUri?.ToString();
            sent = await request.Content!.ReadFromJsonAsync<CreateCustomerRequest>();
            return new HttpResponseMessage(HttpStatusCode.Created) { Content = JsonContent.Create(customer) };
        });
        var request = new CreateCustomerRequest { CustomerName = "Acme", Email = "billing@example.com" };

        var result = await client.Create(request);

        Assert.True(result.Succeeded);
        Assert.Equal(customer.Id, result.Customer!.Id);
        Assert.Equal(HttpMethod.Post, method);
        Assert.Equal("http://localhost/api/customers", requestUri);
        Assert.Equal(request.CustomerName, sent!.CustomerName);
        Assert.Equal(request.Email, sent.Email);
    }

    [Fact]
    public async Task Create_preserves_validation_errors()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = JsonContent.Create(new
            {
                errors = new Dictionary<string, string[]>
                {
                    ["CustomerName"] = ["Customer name is required."],
                    ["general"] = ["Review the request."]
                }
            })
        };

        var result = await CreateClient(response).Create(new CreateCustomerRequest());

        Assert.False(result.Succeeded);
        Assert.Equal(["Customer name is required."], result.Errors["CustomerName"]);
        Assert.Equal(["Review the request."], result.Errors["general"]);
        Assert.Null(result.Message);
    }

    [Fact]
    public async Task Create_returns_retry_message_for_unexpected_response()
    {
        var result = await CreateClient(new HttpResponseMessage(HttpStatusCode.InternalServerError))
            .Create(new CreateCustomerRequest());

        Assert.False(result.Succeeded);
        Assert.Empty(result.Errors);
        Assert.Equal("Could not save customer. Try again.", result.Message);
    }

    [Fact]
    public async Task Create_returns_connection_message_when_api_is_unreachable()
    {
        var client = CreateClient(_ =>
            Task.FromException<HttpResponseMessage>(new HttpRequestException("Unavailable")));

        var result = await client.Create(new CreateCustomerRequest());

        Assert.False(result.Succeeded);
        Assert.Equal("Could not save customer. Check the API connection and try again.", result.Message);
    }

    private static CustomerClient CreateClient(HttpResponseMessage response) =>
        CreateClient(_ => Task.FromResult(response));

    private static CustomerClient CreateClient(Func<HttpRequestMessage, Task<HttpResponseMessage>> sendAsync) =>
        new(new HttpClient(new StubHttpMessageHandler(sendAsync))
        {
            BaseAddress = new Uri("http://localhost")
        });

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> sendAsync) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            sendAsync(request);
    }
}

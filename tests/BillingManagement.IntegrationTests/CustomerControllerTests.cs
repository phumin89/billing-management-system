using System.Net;
using System.Net.Http.Json;
using BillingManagement.Api.Controllers;
using BillingManagement.Application;
using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Contracts.Customers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BillingManagement.IntegrationTests;

public sealed class CustomerControllerTests
{
    [Fact]
    public async Task Post_invalid_request_returns_field_validation_problem()
    {
        var store = new InMemoryCustomerStore();
        await using var app = await StartApplication(store);
        using var client = CreateClient(app);

        var response = await client.PostAsJsonAsync("/api/customers", new CreateCustomerRequest
        {
            CustomerName = " ",
            Email = "invalid"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(["Customer name is required."], problem.Errors["CustomerName"]);
        Assert.Equal(["Email format is invalid."], problem.Errors["Email"]);
        Assert.Empty(store.Customers);
    }

    [Fact]
    public async Task Post_valid_and_duplicate_names_return_created_and_persist_both()
    {
        var store = new InMemoryCustomerStore();
        await using var app = await StartApplication(store);
        using var client = CreateClient(app);
        var request = new CreateCustomerRequest
        {
            CustomerName = " Acme ",
            TaxId = " ",
            Email = " billing@example.com "
        };

        var first = await client.PostAsJsonAsync("/api/customers", request);
        var second = await client.PostAsJsonAsync("/api/customers", request);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        var firstCustomer = await first.Content.ReadFromJsonAsync<CustomerResponse>();
        var secondCustomer = await second.Content.ReadFromJsonAsync<CustomerResponse>();
        Assert.NotNull(firstCustomer);
        Assert.NotNull(secondCustomer);
        Assert.NotEqual(firstCustomer.Id, secondCustomer.Id);
        Assert.Equal("Acme", firstCustomer.CustomerName);
        Assert.Null(firstCustomer.TaxId);
        Assert.Equal("billing@example.com", firstCustomer.Email);
        Assert.Equal(2, store.Customers.Count);
    }

    private static async Task<WebApplication> StartApplication(ICustomerStore store)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Production
        });
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddControllers().AddApplicationPart(typeof(CustomersController).Assembly);
        builder.Services.AddSingleton(store);
        builder.Services.AddBillingManagementApplication();
        builder.Services.AddProblemDetails();

        var app = builder.Build();
        app.UseExceptionHandler();
        app.MapControllers();
        await app.StartAsync();
        return app;
    }

    private static HttpClient CreateClient(WebApplication app)
    {
        var addresses = app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!;
        return new HttpClient { BaseAddress = new Uri(addresses.Addresses.Single()) };
    }

    private sealed class InMemoryCustomerStore : ICustomerStore
    {
        public List<CustomerRecord> Customers { get; } = [];

        public Task Add(CustomerRecord customer, CancellationToken cancellationToken = default)
        {
            this.Customers.Add(customer);
            return Task.CompletedTask;
        }
    }
}

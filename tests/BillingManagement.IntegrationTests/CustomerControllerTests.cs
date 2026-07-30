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
    public async Task Get_empty_store_returns_ok_with_empty_array()
    {
        var store = new InMemoryCustomerStore();
        await using var app = await StartApplication(store);
        using var client = CreateClient(app);

        var response = await client.GetAsync("/api/customers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var customers = await response.Content.ReadFromJsonAsync<CustomerResponse[]>();
        Assert.NotNull(customers);
        Assert.Empty(customers);
    }

    [Fact]
    public async Task Get_existing_customers_returns_all_fields_ordered_by_name_then_id()
    {
        var firstDuplicateId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var secondDuplicateId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var store = new InMemoryCustomerStore();
        store.Customers.Add(new CustomerRecord(
            secondDuplicateId, "Duplicate", null, null, null, null, null, null, null, null, null, null));
        store.Customers.Add(new CustomerRecord(
            Guid.Parse("33333333-3333-3333-3333-333333333333"), "Alpha", "TAX-1",
            "billing@example.com", "0123", "1 Main Street", "Suite 2", "Bangkok", "10110",
            "Thailand", "Jane", "Notes"));
        store.Customers.Add(new CustomerRecord(
            firstDuplicateId, "Duplicate", null, null, null, null, null, null, null, null, null, null));
        await using var app = await StartApplication(store);
        using var client = CreateClient(app);

        var response = await client.GetAsync("/api/customers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var customers = await response.Content.ReadFromJsonAsync<CustomerResponse[]>();
        Assert.NotNull(customers);
        Assert.Equal(["Alpha", "Duplicate", "Duplicate"], customers.Select(customer => customer.CustomerName));
        Assert.Equal(firstDuplicateId, customers[1].Id);
        Assert.Equal(secondDuplicateId, customers[2].Id);
        Assert.Equal("TAX-1", customers[0].TaxId);
        Assert.Equal("billing@example.com", customers[0].Email);
        Assert.Equal("0123", customers[0].Phone);
        Assert.Equal("1 Main Street", customers[0].BillingAddressLine1);
        Assert.Equal("Suite 2", customers[0].BillingAddressLine2);
        Assert.Equal("Bangkok", customers[0].CityProvinceState);
        Assert.Equal("10110", customers[0].PostalCode);
        Assert.Equal("Thailand", customers[0].Country);
        Assert.Equal("Jane", customers[0].ContactName);
        Assert.Equal("Notes", customers[0].Notes);
    }

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

    [Fact]
    public async Task Put_invalid_request_returns_field_validation_problem_without_updating()
    {
        var store = new InMemoryCustomerStore();
        await using var app = await StartApplication(store);
        using var client = CreateClient(app);

        var response = await client.PutAsJsonAsync($"/api/customers/{Guid.NewGuid()}", new UpdateCustomerRequest
        {
            CustomerName = " ",
            Email = "invalid"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(["Customer name is required."], problem.Errors["CustomerName"]);
        Assert.Equal(["Email format is invalid."], problem.Errors["Email"]);
        Assert.Empty(store.Customers);
    }

    [Fact]
    public async Task Put_missing_customer_returns_not_found()
    {
        var store = new InMemoryCustomerStore();
        await using var app = await StartApplication(store);
        using var client = CreateClient(app);

        var response = await client.PutAsJsonAsync($"/api/customers/{Guid.NewGuid()}", ValidUpdateRequest("Missing"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_route_id_is_authoritative_and_valid_update_returns_persisted_response()
    {
        var routeId = Guid.NewGuid();
        var store = new InMemoryCustomerStore();
        store.Customers.Add(new CustomerRecord(routeId, "Old", null, null, null, null, null, null, null, null, null, null));
        await using var app = await StartApplication(store);
        using var client = CreateClient(app);

        var response = await client.PutAsJsonAsync($"/api/customers/{routeId}", new UpdateCustomerRequest
        {
            CustomerName = " Updated ",
            TaxId = " ",
            Email = " billing@example.com "
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var customer = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        Assert.NotNull(customer);
        Assert.Equal(routeId, customer.Id);
        Assert.Equal("Updated", customer.CustomerName);
        Assert.Null(customer.TaxId);
        Assert.Equal("billing@example.com", customer.Email);
        var persisted = Assert.Single(store.Customers);
        Assert.Equal(customer.Id, persisted.Id);
        Assert.Equal(customer.CustomerName, persisted.CustomerName);
        Assert.Equal(customer.Email, persisted.Email);
    }

    [Fact]
    public async Task Put_allows_duplicate_customer_name()
    {
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var store = new InMemoryCustomerStore();
        store.Customers.Add(new CustomerRecord(firstId, "Duplicate", null, null, null, null, null, null, null, null, null, null));
        store.Customers.Add(new CustomerRecord(secondId, "Original", null, null, null, null, null, null, null, null, null, null));
        await using var app = await StartApplication(store);
        using var client = CreateClient(app);

        var response = await client.PutAsJsonAsync($"/api/customers/{secondId}", ValidUpdateRequest("Duplicate"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, store.Customers.Count(customer => customer.CustomerName == "Duplicate"));
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

    private static UpdateCustomerRequest ValidUpdateRequest(string customerName) =>
        new() { CustomerName = customerName };

    private sealed class InMemoryCustomerStore : ICustomerStore
    {
        public List<CustomerRecord> Customers { get; } = [];

        public Task<IReadOnlyList<CustomerRecord>> List(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<CustomerRecord>>(
                this.Customers
                    .OrderBy(customer => customer.CustomerName)
                    .ThenBy(customer => customer.Id)
                    .ToList());

        public Task Add(CustomerRecord customer, CancellationToken cancellationToken = default)
        {
            this.Customers.Add(customer);
            return Task.CompletedTask;
        }

        public Task<bool> Update(CustomerRecord customer, CancellationToken cancellationToken = default)
        {
            var index = this.Customers.FindIndex(existing => existing.Id == customer.Id);
            if (index < 0)
            {
                return Task.FromResult(false);
            }

            this.Customers[index] = customer;
            return Task.FromResult(true);
        }
    }
}

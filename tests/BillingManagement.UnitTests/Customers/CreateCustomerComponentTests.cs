using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using BillingManagement.Client.Customers;
using BillingManagement.Client.Pages.Customers;
using BillingManagement.Contracts.Customers;
using Microsoft.AspNetCore.Components;

namespace BillingManagement.UnitTests.Customers;

public sealed class CreateCustomerComponentTests
{
    [Fact]
    public async Task Save_success_adds_api_customer_clears_form_and_navigates()
    {
        var saved = new CustomerResponse { Id = Guid.NewGuid(), CustomerName = "Acme" };
        var component = CreateComponent(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = JsonContent.Create(saved)
        }));
        Form(component).CustomerName = " Acme ";

        await Save(component);

        Assert.Equal(saved.Id, State(component).Customers.Single().Id);
        Assert.Null(Form(component).CustomerName);
        Assert.Equal("http://localhost/customers", Navigation(component).Uri);
    }

    [Fact]
    public async Task Save_validation_preserves_form_and_separates_field_and_general_errors()
    {
        var component = CreateComponent(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = JsonContent.Create(new
            {
                errors = new Dictionary<string, string[]>
                {
                    ["CustomerName"] = ["Customer name is required."],
                    ["general"] = ["Review the request."]
                }
            })
        }));
        Form(component).CustomerName = " ";

        await Save(component);

        Assert.Equal(" ", Form(component).CustomerName);
        Assert.Equal("Customer name is required.", Invoke<string>(component, "FieldError", "CustomerName"));
        Assert.Equal("Review the request.", Invoke<string>(component, "GeneralError"));
        Assert.Equal("http://localhost/customers/create", Navigation(component).Uri);
    }

    [Fact]
    public async Task Save_failure_preserves_every_value_and_shows_retry_message()
    {
        var component = CreateComponent(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));
        Form(component).CustomerName = "Acme";
        Form(component).Notes = "Keep this value";

        await Save(component);

        Assert.Equal("Acme", Form(component).CustomerName);
        Assert.Equal("Keep this value", Form(component).Notes);
        Assert.Equal("Could not save customer. Try again.", Invoke<string>(component, "GeneralError"));
    }

    [Fact]
    public async Task Save_blocks_duplicate_request_while_submitting()
    {
        var response = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        var requestCount = 0;
        var component = CreateComponent(_ =>
        {
            Interlocked.Increment(ref requestCount);
            return response.Task;
        });

        var first = Save(component);
        var duplicate = Save(component);

        Assert.Equal(1, requestCount);
        Assert.True(Field<bool>(component, "isSubmitting"));
        response.SetResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        await Task.WhenAll(first, duplicate);
        Assert.False(Field<bool>(component, "isSubmitting"));
    }

    private static CreateCustomer CreateComponent(Func<HttpRequestMessage, Task<HttpResponseMessage>> sendAsync)
    {
        var component = new CreateCustomer();
        Property(component, "Client").SetValue(component, new CustomerClient(new HttpClient(
            new StubHttpMessageHandler(sendAsync))
        { BaseAddress = new Uri("http://localhost") }));
        Property(component, "Navigation").SetValue(component, new TestNavigationManager());
        Property(component, "CustomerState").SetValue(component, new CustomerSessionState());
        return component;
    }

    private static Task Save(CreateCustomer component) => Invoke<Task>(component, "SaveCustomer");
    private static CreateCustomerRequest Form(CreateCustomer component) => Field<CreateCustomerRequest>(component, "form");
    private static CustomerSessionState State(CreateCustomer component) =>
        (CustomerSessionState)Property(component, "CustomerState").GetValue(component)!;
    private static TestNavigationManager Navigation(CreateCustomer component) =>
        (TestNavigationManager)Property(component, "Navigation").GetValue(component)!;
    private static T Field<T>(CreateCustomer component, string name) =>
        (T)component.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(component)!;
    private static PropertyInfo Property(CreateCustomer component, string name) =>
        component.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static T Invoke<T>(CreateCustomer component, string name, params object[] arguments) =>
        (T)component.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(component, arguments)!;

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> sendAsync) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => sendAsync(request);
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager() => this.Initialize("http://localhost/", "http://localhost/customers/create");
        protected override void NavigateToCore(string uri, bool forceLoad) =>
            this.Uri = this.ToAbsoluteUri(uri).AbsoluteUri;
    }
}

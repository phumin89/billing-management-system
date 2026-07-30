using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using BillingManagement.Client.Customers;
using BillingManagement.Contracts.Customers;
using CustomersPage = BillingManagement.Client.Pages.Customers.Customers;

namespace BillingManagement.UnitTests.Customers;

public sealed class CustomersComponentTests
{
    [Fact]
    public void Initialize_and_begin_edit_populate_a_copy_of_existing_customer_values()
    {
        var component = CreateComponent();

        Invoke(component, "OnInitialized");
        var customer = State(component).Customers.Single();
        Invoke(component, "BeginEdit", customer);

        var form = Field<CreateCustomerRequest>(component, "editForm");
        Assert.Equal(customer.CustomerName, form.CustomerName);
        Assert.Equal(customer.TaxId, form.TaxId);
        Assert.Equal(customer.Email, form.Email);
        Assert.Equal(customer.Phone, form.Phone);
        Assert.Equal(customer.BillingAddressLine1, form.BillingAddressLine1);
        Assert.Equal(customer.BillingAddressLine2, form.BillingAddressLine2);
        Assert.Equal(customer.CityProvinceState, form.CityProvinceState);
        Assert.Equal(customer.PostalCode, form.PostalCode);
        Assert.Equal(customer.Country, form.Country);
        Assert.Equal(customer.ContactName, form.ContactName);
        Assert.Equal(customer.Notes, form.Notes);
        Assert.True(Property<bool>(component, "IsEditing"));
    }

    [Fact]
    public void Cancel_edit_discards_changes_and_restores_customer_values()
    {
        var component = CreateComponent();
        Invoke(component, "OnInitialized");
        var customer = State(component).Customers.Single();
        var originalName = customer.CustomerName;
        Invoke(component, "BeginEdit", customer);
        Field<CreateCustomerRequest>(component, "editForm").CustomerName = "Changed locally";

        Invoke(component, "CancelEdit");

        Assert.False(Property<bool>(component, "IsEditing"));
        Assert.Equal(originalName, customer.CustomerName);
    }

    [Fact]
    public async Task Save_sends_selected_id_replaces_current_state_and_exits_edit()
    {
        Guid? requestedId = null;
        var updated = new CustomerResponse { Id = Guid.NewGuid(), CustomerName = "Updated by API" };
        var component = CreateEditingComponent(request =>
        {
            requestedId = Guid.Parse(request.RequestUri!.Segments.Last());
            updated.Id = requestedId.Value;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(updated)
            });
        });
        var original = State(component).Customers.Single();
        Field<CreateCustomerRequest>(component, "editForm").CustomerName = "Entered value";

        await Invoke<Task>(component, "SaveCustomer");

        Assert.Equal(original.Id, requestedId);
        Assert.Equal("Updated by API", State(component).Customers.Single().CustomerName);
        Assert.False(Property<bool>(component, "IsEditing"));
    }

    [Fact]
    public async Task Save_validation_preserves_values_and_separates_field_and_general_errors()
    {
        var component = CreateEditingComponent(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
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
        Field<CreateCustomerRequest>(component, "editForm").CustomerName = " ";

        await Invoke<Task>(component, "SaveCustomer");

        Assert.Equal(" ", Field<CreateCustomerRequest>(component, "editForm").CustomerName);
        Assert.Equal("Customer name is required.", Invoke<string>(component, "FieldError", "CustomerName"));
        Assert.Equal("Review the request.", Invoke<string>(component, "GeneralError"));
        Assert.True(Property<bool>(component, "IsEditing"));
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, "Customer was not found. Refresh and try again.")]
    [InlineData(HttpStatusCode.InternalServerError, "Could not update customer. Try again.")]
    public async Task Save_failure_keeps_entered_values_and_shows_retry(
        HttpStatusCode statusCode,
        string expectedMessage)
    {
        var component = CreateEditingComponent(_ =>
            Task.FromResult(new HttpResponseMessage(statusCode)));
        Field<CreateCustomerRequest>(component, "editForm").Notes = "Keep this value";

        await Invoke<Task>(component, "SaveCustomer");

        Assert.Equal("Keep this value", Field<CreateCustomerRequest>(component, "editForm").Notes);
        Assert.Equal(expectedMessage, Invoke<string>(component, "GeneralError"));
        Assert.True(Property<bool>(component, "IsEditing"));
    }

    [Fact]
    public async Task Save_blocks_duplicate_request_while_submitting()
    {
        var response = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        var requestCount = 0;
        var component = CreateEditingComponent(_ =>
        {
            Interlocked.Increment(ref requestCount);
            return response.Task;
        });

        var first = Invoke<Task>(component, "SaveCustomer");
        var duplicate = Invoke<Task>(component, "SaveCustomer");

        Assert.Equal(1, requestCount);
        Assert.True(Field<bool>(component, "isSubmitting"));
        response.SetResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        await Task.WhenAll(first, duplicate);
        Assert.False(Field<bool>(component, "isSubmitting"));
    }

    [Fact]
    public void Cancel_does_not_send_request_or_mutate_current_state()
    {
        var requestCount = 0;
        var component = CreateEditingComponent(_ =>
        {
            Interlocked.Increment(ref requestCount);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });
        var customer = State(component).Customers.Single();
        var originalName = customer.CustomerName;
        Field<CreateCustomerRequest>(component, "editForm").CustomerName = "Changed locally";

        Invoke(component, "CancelEdit");

        Assert.Equal(0, requestCount);
        Assert.Equal(originalName, State(component).Customers.Single().CustomerName);
        Assert.False(Property<bool>(component, "IsEditing"));
    }

    private static CustomersPage CreateComponent()
    {
        var component = new CustomersPage();
        var stateProperty = component.GetType().GetProperty(
            "CustomerState",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(stateProperty);
        stateProperty.SetValue(component, new CustomerSessionState());
        return component;
    }

    private static CustomersPage CreateEditingComponent(Func<HttpRequestMessage, Task<HttpResponseMessage>> sendAsync)
    {
        var component = CreateComponent();
        var client = new CustomerClient(new HttpClient(new StubHttpMessageHandler(sendAsync))
        {
            BaseAddress = new Uri("http://localhost")
        });
        Property(component, "Client").SetValue(component, client);
        Invoke(component, "OnInitialized");
        Invoke(component, "BeginEdit", State(component).Customers.Single());
        return component;
    }

    private static CustomerSessionState State(CustomersPage component)
    {
        var property = component.GetType().GetProperty(
            "CustomerState",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(property);
        return Assert.IsType<CustomerSessionState>(property.GetValue(component));
    }

    private static T Field<T>(CustomersPage component, string name)
    {
        var field = component.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        return Assert.IsType<T>(field.GetValue(component));
    }

    private static PropertyInfo Property(CustomersPage component, string name) =>
        component.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static T Property<T>(CustomersPage component, string name)
    {
        var property = component.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(property);
        return Assert.IsType<T>(property.GetValue(component));
    }

    private static void Invoke(CustomersPage component, string name, params object[] arguments)
    {
        var method = component.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);
        method.Invoke(component, arguments);
    }

    private static T Invoke<T>(CustomersPage component, string name, params object[] arguments) =>
        (T)component.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(component, arguments)!;

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> sendAsync) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => sendAsync(request);
    }
}

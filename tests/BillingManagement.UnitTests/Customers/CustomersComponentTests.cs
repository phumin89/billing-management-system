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
    public async Task Initialize_and_begin_edit_populate_a_copy_of_existing_customer_values()
    {
        var component = CreateComponent();

        await Invoke<Task>(component, "OnInitializedAsync");
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
    public async Task Cancel_edit_discards_changes_and_restores_customer_values()
    {
        var component = CreateComponent();
        await Invoke<Task>(component, "OnInitializedAsync");
        var customer = State(component).Customers.Single();
        var originalName = customer.CustomerName;
        Invoke(component, "BeginEdit", customer);
        Field<CreateCustomerRequest>(component, "editForm").CustomerName = "Changed locally";

        Invoke(component, "CancelEdit");

        Assert.False(Property<bool>(component, "IsEditing"));
        Assert.Equal(originalName, customer.CustomerName);
    }

    [Fact]
    public async Task Initialize_replaces_stale_rows_with_api_list_and_marks_loading_complete()
    {
        var first = new CustomerResponse { Id = Guid.NewGuid(), CustomerName = "Alpha", Email = "alpha@example.com" };
        var second = new CustomerResponse { Id = Guid.NewGuid(), CustomerName = "Duplicate" };
        var state = new CustomerSessionState();
        state.Add(new CustomerResponse { Id = Guid.NewGuid(), CustomerName = "Stale" });
        var component = CreateComponent(state, _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new[] { first, second })
        }));

        await Invoke<Task>(component, "OnInitializedAsync");

        Assert.Equal([first.Id, second.Id], State(component).Customers.Select(customer => customer.Id));
        Assert.True(State(component).IsLoaded);
        Assert.False(Field<bool>(component, "isLoading"));
        Assert.Null(FieldValue(component, "loadError"));
    }

    [Fact]
    public async Task Initialize_empty_response_marks_loaded_without_showing_stale_rows()
    {
        var state = new CustomerSessionState();
        state.Add(new CustomerResponse { Id = Guid.NewGuid(), CustomerName = "Stale" });
        var component = CreateComponent(state, _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(Array.Empty<CustomerResponse>())
        }));

        await Invoke<Task>(component, "OnInitializedAsync");

        Assert.True(State(component).IsLoaded);
        Assert.Empty(State(component).Customers);
        Assert.False(Field<bool>(component, "isLoading"));
    }

    [Fact]
    public async Task Retry_after_failure_replaces_rows_without_duplicates()
    {
        var customer = new CustomerResponse { Id = Guid.NewGuid(), CustomerName = "Recovered" };
        var requestCount = 0;
        var component = CreateComponent(new CustomerSessionState(), _ =>
        {
            requestCount++;
            return Task.FromResult(requestCount == 1
                ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
                : new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new[] { customer })
                });
        });

        await Invoke<Task>(component, "OnInitializedAsync");
        Assert.Equal("Could not load customers. Try again.", Field<string?>(component, "loadError"));

        await Invoke<Task>(component, "RetryLoad");

        Assert.Equal(2, requestCount);
        Assert.Equal(customer.Id, State(component).Customers.Single().Id);
        Assert.Null(FieldValue(component, "loadError"));
    }

    [Fact]
    public async Task Initialize_with_loaded_state_skips_reload_and_preserves_created_customer()
    {
        var state = new CustomerSessionState();
        state.ReplaceAll([new CustomerResponse { Id = Guid.NewGuid(), CustomerName = "Existing" }]);
        var created = new CustomerResponse { Id = Guid.NewGuid(), CustomerName = "Created" };
        state.Add(created);
        var requestCount = 0;
        var component = CreateComponent(state, _ =>
        {
            requestCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        await Invoke<Task>(component, "OnInitializedAsync");

        Assert.Equal(0, requestCount);
        Assert.Equal(["Existing", "Created"], State(component).Customers.Select(customer => customer.CustomerName));
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

    [Fact]
    public async Task Delete_confirmation_targets_exact_customer_and_cancel_sends_no_request()
    {
        var requestCount = 0;
        var component = CreateDeleteComponent(_ =>
        {
            requestCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        });
        var selected = State(component).Customers[1];

        Invoke(component, "BeginDelete", selected);
        Assert.Same(selected, Field<CustomerResponse>(component, "deletingCustomer"));

        await Invoke<Task>(component, "CloseDeleteSnackbar");

        Assert.Equal(0, requestCount);
        Assert.Equal(2, State(component).Customers.Count);
        Assert.False(Field<bool>(component, "showDeleteSnackbar"));
    }

    [Fact]
    public async Task Confirm_delete_removes_only_selected_customer_after_no_content()
    {
        Guid? requestedId = null;
        var component = CreateDeleteComponent(request =>
        {
            requestedId = Guid.Parse(request.RequestUri!.Segments.Last());
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        });
        var selected = State(component).Customers[1];
        Invoke(component, "BeginDelete", selected);

        await Invoke<Task>(component, "ConfirmDelete");

        Assert.Equal(selected.Id, requestedId);
        Assert.Equal(["Northstar Studio"], State(component).Customers.Select(customer => customer.CustomerName));
        Assert.False(Field<bool>(component, "showDeleteSnackbar"));
    }

    [Fact]
    public async Task Confirm_delete_removes_stale_customer_and_shows_not_found_message()
    {
        var component = CreateDeleteComponent(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)));
        Invoke(component, "BeginDelete", State(component).Customers[0]);

        await Invoke<Task>(component, "ConfirmDelete");

        Assert.Single(State(component).Customers);
        Assert.Equal(
            "Customer was not found and was removed from this list.",
            Field<string>(component, "deleteMessage"));
        Assert.False(Field<bool>(component, "deleteMessageIsError"));
    }

    [Theory]
    [InlineData(
        HttpStatusCode.Conflict,
        "Customer is used by quotations or invoices and cannot be deleted.")]
    [InlineData(HttpStatusCode.InternalServerError, "Could not delete customer. Try again.")]
    public async Task Confirm_delete_failure_keeps_customer_and_shows_retryable_message(
        HttpStatusCode statusCode,
        string expectedMessage)
    {
        var component = CreateDeleteComponent(_ =>
            Task.FromResult(new HttpResponseMessage(statusCode)));
        var selected = State(component).Customers[0];
        Invoke(component, "BeginDelete", selected);

        await Invoke<Task>(component, "ConfirmDelete");

        Assert.Contains(State(component).Customers, customer => customer.Id == selected.Id);
        Assert.Equal(expectedMessage, Field<string>(component, "deleteMessage"));
        Assert.True(Field<bool>(component, "deleteMessageIsError"));
        Assert.False(Field<bool>(component, "showDeleteSnackbar"));
    }

    [Fact]
    public async Task Confirm_delete_blocks_duplicate_request_while_deleting()
    {
        var response = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        var requestCount = 0;
        var component = CreateDeleteComponent(_ =>
        {
            Interlocked.Increment(ref requestCount);
            return response.Task;
        });
        Invoke(component, "BeginDelete", State(component).Customers[0]);

        var first = Invoke<Task>(component, "ConfirmDelete");
        var duplicate = Invoke<Task>(component, "ConfirmDelete");

        Assert.Equal(1, requestCount);
        Assert.True(Field<bool>(component, "isDeleting"));
        response.SetResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        await Task.WhenAll(first, duplicate);
        Assert.False(Field<bool>(component, "isDeleting"));
    }

    private static CustomersPage CreateComponent()
    {
        var state = new CustomerSessionState();
        state.ReplaceAll([SampleCustomer()]);
        return CreateComponent(state, _ => throw new InvalidOperationException("Loaded state must not call the API."));
    }

    private static CustomersPage CreateComponent(
        CustomerSessionState state,
        Func<HttpRequestMessage, Task<HttpResponseMessage>> sendAsync)
    {
        var component = new CustomersPage();
        Property(component, "CustomerState").SetValue(component, state);
        Property(component, "Client").SetValue(component, new CustomerClient(new HttpClient(
            new StubHttpMessageHandler(sendAsync))
        { BaseAddress = new Uri("http://localhost") }));
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
        Invoke(component, "BeginEdit", State(component).Customers.Single());
        return component;
    }

    private static CustomersPage CreateDeleteComponent(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> sendAsync)
    {
        var state = new CustomerSessionState();
        state.ReplaceAll([
            SampleCustomer(),
            new CustomerResponse { Id = Guid.NewGuid(), CustomerName = "Delete target" }
        ]);
        return CreateComponent(state, sendAsync);
    }

    private static CustomerResponse SampleCustomer() =>
        new()
        {
            Id = Guid.Parse("86fb6f33-5327-4d89-ae07-a678b2955970"),
            CustomerName = "Northstar Studio",
            TaxId = "TH-0105560123456",
            Email = "billing@northstar.example",
            Phone = "+66 2 555 0142",
            BillingAddressLine1 = "88 Wireless Road",
            BillingAddressLine2 = "Unit 1204",
            CityProvinceState = "Bangkok",
            PostalCode = "10330",
            Country = "Thailand",
            ContactName = "Maya Chen",
            Notes = "Monthly billing contact"
        };

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

    private static object? FieldValue(CustomersPage component, string name) =>
        component.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(component);

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

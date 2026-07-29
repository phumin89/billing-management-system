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
}

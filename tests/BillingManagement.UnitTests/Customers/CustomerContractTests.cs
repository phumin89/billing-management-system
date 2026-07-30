using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using BillingManagement.Contracts.Customers;

namespace BillingManagement.UnitTests.Customers;

public sealed class CustomerContractTests
{
    private static readonly string[] CustomerPropertyNames =
    [
        "BillingAddressLine1",
        "BillingAddressLine2",
        "CityProvinceState",
        "ContactName",
        "Country",
        "CustomerName",
        "Email",
        "Notes",
        "Phone",
        "PostalCode",
        "TaxId"
    ];

    [Fact]
    public void Create_customer_request_has_exact_nullable_transport_shape()
    {
        AssertContractShape(typeof(CreateCustomerRequest), includeId: false);
    }

    [Fact]
    public void Update_customer_request_has_exact_nullable_transport_shape()
    {
        AssertContractShape(typeof(UpdateCustomerRequest), includeId: false);
    }

    [Fact]
    public void Customer_response_has_exact_nullable_transport_shape()
    {
        AssertContractShape(typeof(CustomerResponse), includeId: true);
    }

    [Fact]
    public void Create_customer_request_serializes_web_field_names_and_null_values()
    {
        var json = JsonSerializer.SerializeToElement(
            new CreateCustomerRequest(),
            JsonSerializerOptions.Web);

        AssertJsonShape(json, includeId: false);
        Assert.All(json.EnumerateObject(), property => Assert.Equal(JsonValueKind.Null, property.Value.ValueKind));
    }

    [Fact]
    public void Update_customer_request_serializes_web_field_names_and_null_values()
    {
        var json = JsonSerializer.SerializeToElement(
            new UpdateCustomerRequest(),
            JsonSerializerOptions.Web);

        AssertJsonShape(json, includeId: false);
        Assert.All(json.EnumerateObject(), property => Assert.Equal(JsonValueKind.Null, property.Value.ValueKind));
    }

    [Fact]
    public void Customer_response_serializes_web_field_names_and_null_values()
    {
        var id = Guid.Parse("c5ca172f-9f1c-4f49-a20d-658b609f23e1");
        var json = JsonSerializer.SerializeToElement(
            new CustomerResponse { Id = id },
            JsonSerializerOptions.Web);

        AssertJsonShape(json, includeId: true);
        Assert.Equal(id, json.GetProperty("id").GetGuid());
        Assert.All(
            json.EnumerateObject().Where(property => property.Name != "id"),
            property => Assert.Equal(JsonValueKind.Null, property.Value.ValueKind));
    }

    private static void AssertContractShape(Type contractType, bool includeId)
    {
        var properties = contractType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var expectedNames = includeId ? CustomerPropertyNames.Append("Id") : CustomerPropertyNames;

        Assert.Equal(expectedNames.Order(), properties.Select(property => property.Name).Order());

        foreach (var property in properties)
        {
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.Empty(property.GetCustomAttributes(typeof(ValidationAttribute), inherit: true));

            if (property.Name == "Id")
            {
                Assert.Equal(typeof(Guid), property.PropertyType);
                continue;
            }

            Assert.Equal(typeof(string), property.PropertyType);
            Assert.Equal(NullabilityState.Nullable, new NullabilityInfoContext().Create(property).ReadState);
        }
    }

    private static void AssertJsonShape(JsonElement json, bool includeId)
    {
        var expectedNames = CustomerPropertyNames
            .Select(JsonNamingPolicy.CamelCase.ConvertName)
            .Concat(includeId ? ["id"] : []);

        Assert.Equal(expectedNames.Order(), json.EnumerateObject().Select(property => property.Name).Order());
    }
}

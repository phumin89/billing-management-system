using BillingManagement.Domain;

namespace BillingManagement.UnitTests.Customers;

public sealed class CustomerTests
{
    [Fact]
    public void Create_generates_id_and_normalizes_text()
    {
        var customer = Customer.Create(
            "  Acme Co  ", " ", "  billing@example.com  ", null,
            "  1 Main Street  ", "\t", "  Bangkok  ", " ",
            "  Thailand  ", "  Jane Doe  ", "\r\n");

        Assert.NotEqual(Guid.Empty, customer.Id);
        Assert.Equal("Acme Co", customer.CustomerName);
        Assert.Null(customer.TaxId);
        Assert.Equal("billing@example.com", customer.Email);
        Assert.Null(customer.Phone);
        Assert.Equal("1 Main Street", customer.BillingAddressLine1);
        Assert.Null(customer.BillingAddressLine2);
        Assert.Equal("Bangkok", customer.CityProvinceState);
        Assert.Null(customer.PostalCode);
        Assert.Equal("Thailand", customer.Country);
        Assert.Equal("Jane Doe", customer.ContactName);
        Assert.Null(customer.Notes);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\r\n")]
    [InlineData("\u00a0")]
    public void Create_rejects_blank_customer_name(string? customerName)
    {
        Assert.ThrowsAny<ArgumentException>(() => Customer.Create(customerName!, null, null, null, null, null, null, null, null, null, null));
    }

    [Fact]
    public void Create_rejects_values_over_shared_limits()
    {
        Assert.Throws<ArgumentException>(() => Customer.Create(Over(CustomerConstraints.CustomerNameMaxLength), null, null, null, null, null, null, null, null, null, null));
        Assert.Throws<ArgumentException>(() => Customer.Create("Acme", null, null, null, null, null, null, null, null, null, Over(CustomerConstraints.NotesMaxLength)));
    }

    [Fact]
    public void Rehydrate_preserves_persisted_id_and_invariants()
    {
        var id = Guid.NewGuid();

        var customer = Customer.Rehydrate(id, " Acme ", null, null, null, null, null, null, null, null, null, null);

        Assert.Equal(id, customer.Id);
        Assert.Equal("Acme", customer.CustomerName);
        Assert.Throws<ArgumentException>(() => Customer.Rehydrate(Guid.Empty, "Acme", null, null, null, null, null, null, null, null, null, null));
    }

    [Fact]
    public void Update_preserves_id_and_normalizes_text()
    {
        var id = Guid.NewGuid();
        var customer = Customer.Rehydrate(id, "Old name", null, null, null, null, null, null, null, null, null, null);

        customer.Update(" New name ", " ", " billing@example.com ", "\t", null, null, " Bangkok ", null, null, null, null);

        Assert.Equal(id, customer.Id);
        Assert.Equal("New name", customer.CustomerName);
        Assert.Null(customer.TaxId);
        Assert.Equal("billing@example.com", customer.Email);
        Assert.Null(customer.Phone);
        Assert.Equal("Bangkok", customer.CityProvinceState);
    }

    private static string Over(int maximumLength) => new('x', maximumLength + 1);
}

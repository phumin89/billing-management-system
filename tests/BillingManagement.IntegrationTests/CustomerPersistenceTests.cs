using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Domain;
using BillingManagement.Infrastructure.Customers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BillingManagement.IntegrationTests;

public sealed class CustomerPersistenceTests
{
    [Fact]
    public async Task Migration_persists_duplicate_names_and_optional_blanks_as_null()
    {
        var databaseName = SqlServerIntegrationTestDatabase.CreateDatabaseName();

        try
        {
            await using var context = SqlServerIntegrationTestDatabase.CreateContext(databaseName);
            await context.Database.MigrateAsync();
            context.Customers.Add(Customer.Create(" Acme ", " ", null, "\t", null, null, null, null, null, null, null));
            context.Customers.Add(Customer.Create("Acme", null, null, null, null, null, null, null, null, null, null));

            await context.SaveChangesAsync();
            var customers = await context.Customers.OrderBy(customer => customer.Id).ToListAsync();

            Assert.Equal(2, customers.Count);
            Assert.All(customers, customer => Assert.Equal("Acme", customer.CustomerName));
            Assert.All(customers, customer => Assert.Null(customer.TaxId));
            Assert.All(customers, customer => Assert.Null(customer.Phone));
        }
        finally
        {
            await SqlServerIntegrationTestDatabase.Delete(databaseName);
        }
    }

    [Theory]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(13)]
    [InlineData(160)]
    public async Task Database_rejects_whitespace_only_customer_name(int characterCode)
    {
        var databaseName = SqlServerIntegrationTestDatabase.CreateDatabaseName();

        try
        {
            await using var context = SqlServerIntegrationTestDatabase.CreateContext(databaseName);
            await context.Database.MigrateAsync();

            var command = characterCode switch
            {
                9 => "INSERT INTO [Customers] ([Id], [CustomerName]) VALUES (NEWID(), NCHAR(9));",
                10 => "INSERT INTO [Customers] ([Id], [CustomerName]) VALUES (NEWID(), NCHAR(10));",
                13 => "INSERT INTO [Customers] ([Id], [CustomerName]) VALUES (NEWID(), NCHAR(13));",
                160 => "INSERT INTO [Customers] ([Id], [CustomerName]) VALUES (NEWID(), NCHAR(160));",
                _ => throw new ArgumentOutOfRangeException(nameof(characterCode))
            };

            await Assert.ThrowsAsync<SqlException>(() => context.Database.ExecuteSqlRawAsync(command));
        }
        finally
        {
            await SqlServerIntegrationTestDatabase.Delete(databaseName);
        }
    }

    [Fact]
    public async Task Store_update_persists_normalized_values_and_allows_duplicate_name()
    {
        var databaseName = SqlServerIntegrationTestDatabase.CreateDatabaseName();

        try
        {
            await using var context = SqlServerIntegrationTestDatabase.CreateContext(databaseName);
            await context.Database.MigrateAsync();
            var first = Customer.Create("Duplicate", null, null, null, null, null, null, null, null, null, null);
            var second = Customer.Create("Original", null, null, null, null, null, null, null, null, null, null);
            context.Customers.AddRange(first, second);
            await context.SaveChangesAsync();
            var store = new CustomerStore(context);

            var updated = await store.Update(new CustomerRecord(
                second.Id, " Duplicate ", " ", " billing@example.com ", null,
                null, null, null, null, null, null, null));
            context.ChangeTracker.Clear();
            var persisted = await context.Customers.SingleAsync(customer => customer.Id == second.Id);

            Assert.True(updated);
            Assert.Equal("Duplicate", persisted.CustomerName);
            Assert.Null(persisted.TaxId);
            Assert.Equal("billing@example.com", persisted.Email);
            Assert.Equal(2, await context.Customers.CountAsync(customer => customer.CustomerName == "Duplicate"));
        }
        finally
        {
            await SqlServerIntegrationTestDatabase.Delete(databaseName);
        }
    }
}

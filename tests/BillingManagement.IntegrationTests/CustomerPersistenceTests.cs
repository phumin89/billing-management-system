using BillingManagement.Domain;
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
}

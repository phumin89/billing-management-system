using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Abstractions.Queries;
using BillingManagement.Domain;
using BillingManagement.Infrastructure.Customers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BillingManagement.IntegrationTests;

public sealed class CustomerPersistenceTests
{
    [Fact]
    public async Task Store_get_by_id_filters_entity_before_projecting_record()
    {
        var databaseName = SqlServerIntegrationTestDatabase.CreateDatabaseName();

        try
        {
            await using var context = SqlServerIntegrationTestDatabase.CreateContext(databaseName);
            await context.Database.MigrateAsync();
            var customer = Customer.Create("Acme", null, null, null, null, null, null, null, null, null, null);
            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var record = await new CustomerStore(context).GetById(customer.Id);

            Assert.NotNull(record);
            Assert.Equal(customer.Id, record.Id);
        }
        finally
        {
            await SqlServerIntegrationTestDatabase.Delete(databaseName);
        }
    }

    [Fact]
    public async Task Store_delete_removes_only_selected_customer_and_reports_missing_id()
    {
        var databaseName = SqlServerIntegrationTestDatabase.CreateDatabaseName();

        try
        {
            await using var context = SqlServerIntegrationTestDatabase.CreateContext(databaseName);
            await context.Database.MigrateAsync();
            var deleted = Customer.Create("Delete", null, null, null, null, null, null, null, null, null, null);
            var preserved = Customer.Create(
                "Preserved", "TAX-1", "billing@example.com", "0123", "1 Main Street", "Suite 2",
                "Bangkok", "10110", "Thailand", "Jane", "Notes");
            context.Customers.AddRange(deleted, preserved);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            var store = new CustomerStore(context);

            var found = await store.Delete(deleted.Id);
            var missing = await store.Delete(Guid.NewGuid());
            var remaining = await context.Customers.AsNoTracking().SingleAsync();

            Assert.True(found);
            Assert.False(missing);
            Assert.Equal(preserved.Id, remaining.Id);
            Assert.Equal("Preserved", remaining.CustomerName);
            Assert.Equal("TAX-1", remaining.TaxId);
            Assert.Equal("billing@example.com", remaining.Email);
            Assert.Equal("0123", remaining.Phone);
            Assert.Equal("1 Main Street", remaining.BillingAddressLine1);
            Assert.Equal("Suite 2", remaining.BillingAddressLine2);
            Assert.Equal("Bangkok", remaining.CityProvinceState);
            Assert.Equal("10110", remaining.PostalCode);
            Assert.Equal("Thailand", remaining.Country);
            Assert.Equal("Jane", remaining.ContactName);
            Assert.Equal("Notes", remaining.Notes);
        }
        finally
        {
            await SqlServerIntegrationTestDatabase.Delete(databaseName);
        }
    }

    [Fact]
    public async Task Store_list_returns_all_fields_ordered_by_name_then_id_without_tracking()
    {
        var databaseName = SqlServerIntegrationTestDatabase.CreateDatabaseName();

        try
        {
            await using var context = SqlServerIntegrationTestDatabase.CreateContext(databaseName);
            await context.Database.MigrateAsync();
            var firstDuplicateId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var secondDuplicateId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            context.Customers.AddRange(
                Customer.Rehydrate(
                    secondDuplicateId, "Duplicate", null, null, null, null, null, null, null, null, null, null),
                Customer.Rehydrate(
                    Guid.Parse("33333333-3333-3333-3333-333333333333"), "Alpha", "TAX-1",
                    "billing@example.com", "0123", "1 Main Street", "Suite 2", "Bangkok", "10110",
                    "Thailand", "Jane", "Notes"),
                Customer.Rehydrate(
                    firstDuplicateId, "Duplicate", null, null, null, null, null, null, null, null, null, null));
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
            var store = new CustomerStore(context);

            var page = await store.Search(new CustomerSearchCriteria(), new PageRequest());
            var customers = page.Items;

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
            Assert.Equal(3, page.TotalCount);
            Assert.Empty(context.ChangeTracker.Entries());
        }
        finally
        {
            await SqlServerIntegrationTestDatabase.Delete(databaseName);
        }
    }

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

            var updated = await store.Update(Customer.Create(
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

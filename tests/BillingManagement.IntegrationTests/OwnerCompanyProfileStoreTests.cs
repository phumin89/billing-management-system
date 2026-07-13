using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;
using BillingManagement.Domain;
using BillingManagement.Infrastructure.OwnerCompanyProfiles;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BillingManagement.IntegrationTests;

public sealed class OwnerCompanyProfileStoreTests
{
    [Fact]
    public async Task Database_rejects_blank_values_and_second_profile()
    {
        var databaseName = SqlServerIntegrationTestDatabase.CreateDatabaseName();

        try
        {
            await using var context = SqlServerIntegrationTestDatabase.CreateContext(databaseName);
            await context.Database.MigrateAsync();
            await context.Database.ExecuteSqlRawAsync(InsertSql("Acme Co", "NULL"));

            var duplicate = await Assert.ThrowsAsync<SqlException>(() =>
                context.Database.ExecuteSqlRawAsync(InsertSql("Other Co", "NULL")));
            Assert.Contains(duplicate.Number, new[] { 2601, 2627 });

            var requiredBlank = await Assert.ThrowsAsync<SqlException>(() =>
                context.Database.ExecuteSqlRawAsync(
                    "UPDATE [OwnerCompanyProfiles] SET [CompanyName] = N' ';"));
            Assert.Equal(547, requiredBlank.Number);

            var optionalBlank = await Assert.ThrowsAsync<SqlException>(() =>
                context.Database.ExecuteSqlRawAsync(
                    "UPDATE [OwnerCompanyProfiles] SET [AddressLine2] = N'  ';"));
            Assert.Equal(547, optionalBlank.Number);
        }
        finally
        {
            await SqlServerIntegrationTestDatabase.Delete(databaseName);
        }
    }

    [Fact]
    public async Task Concurrent_create_persists_one_profile_and_returns_one_duplicate()
    {
        var databaseName = SqlServerIntegrationTestDatabase.CreateDatabaseName();

        try
        {
            await using (var setup = SqlServerIntegrationTestDatabase.CreateContext(databaseName))
            {
                await setup.Database.MigrateAsync();
            }

            await using var firstContext = SqlServerIntegrationTestDatabase.CreateContext(databaseName);
            await using var secondContext = SqlServerIntegrationTestDatabase.CreateContext(databaseName);
            var firstHandler = new CreateOwnerCompanyProfileHandler(new OwnerCompanyProfileStore(firstContext));
            var secondHandler = new CreateOwnerCompanyProfileHandler(new OwnerCompanyProfileStore(secondContext));

            var results = await Task.WhenAll(
                firstHandler.Handle(ValidCommand("First Co")),
                secondHandler.Handle(ValidCommand("Second Co")));

            Assert.Single(results, result => result.Succeeded);
            Assert.Single(results, result =>
                !result.Succeeded && result.Errors["Profile"].Contains("Owner company profile already exists."));

            await using var verification = SqlServerIntegrationTestDatabase.CreateContext(databaseName);
            Assert.Equal(1, await verification.OwnerCompanyProfiles.CountAsync());
        }
        finally
        {
            await SqlServerIntegrationTestDatabase.Delete(databaseName);
        }
    }

    [Fact]
    public async Task Get_fails_loudly_when_legacy_database_contains_multiple_profiles()
    {
        var databaseName = SqlServerIntegrationTestDatabase.CreateDatabaseName();

        try
        {
            await using var context = SqlServerIntegrationTestDatabase.CreateContext(databaseName);
            await context.Database.MigrateAsync("20260707133000_RenameOwnerCompanyCityProvinceState");
            await context.Database.ExecuteSqlRawAsync(
                "ALTER TABLE [OwnerCompanyProfiles] ADD [SingletonKey] tinyint NOT NULL CONSTRAINT [DF_Test_SingletonKey] DEFAULT 1;");
            await context.Database.ExecuteSqlRawAsync(InsertSql("First Co", "NULL"));
            await context.Database.ExecuteSqlRawAsync(InsertSql("Second Co", "NULL"));
            var store = new OwnerCompanyProfileStore(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() => store.GetAsync());
        }
        finally
        {
            await SqlServerIntegrationTestDatabase.Delete(databaseName);
        }
    }

    private static CreateOwnerCompanyProfileCommand ValidCommand(string companyName) =>
        new(
            companyName, "1 Main Street", null, "Bangkok", "10110", "Thailand",
            null, null, null, null, null, null);

    private static string InsertSql(string companyName, string addressLine2) =>
        $"""
        INSERT INTO [OwnerCompanyProfiles]
            ([Id], [CompanyName], [AddressLine1], [AddressLine2], [CityProvinceState],
             [PostalCode], [Country], [TaxId], [Phone], [Email], [Website],
             [LogoReference], [RegistrationNumber])
        VALUES
            (NEWID(), N'{companyName}', N'1 Main Street', {addressLine2}, N'Bangkok',
             N'10110', N'Thailand', NULL, NULL, NULL, NULL, NULL, NULL);
        """;
}

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BillingManagement.IntegrationTests;

public sealed class OwnerCompanyProfileMigrationTests
{
    private const string PreviousMigration = "20260707133000_RenameOwnerCompanyCityProvinceState";

    [Fact]
    public async Task Migration_trims_valid_data_and_normalizes_optional_blanks_to_null()
    {
        var databaseName = SqlServerIntegrationTestDatabase.CreateDatabaseName();

        try
        {
            await using var context = SqlServerIntegrationTestDatabase.CreateContext(databaseName);
            await context.Database.MigrateAsync(PreviousMigration);
            await context.Database.ExecuteSqlRawAsync("""
                INSERT INTO [OwnerCompanyProfiles]
                    ([Id], [CompanyName], [AddressLine1], [AddressLine2], [CityProvinceState],
                     [PostalCode], [Country], [TaxId], [Phone], [Email], [Website],
                     [LogoReference], [RegistrationNumber])
                VALUES
                    (NEWID(), NCHAR(9) + N'Acme' + NCHAR(9) + N'Co' + NCHAR(160),
                     NCHAR(10) + N'1 Main Street' + NCHAR(13), NCHAR(9),
                     NCHAR(11) + N'Bangkok' + NCHAR(12), NCHAR(13) + N'10110' + NCHAR(10),
                     NCHAR(160) + N'Thailand' + NCHAR(9), NCHAR(160) + N'TAX-1' + NCHAR(13),
                     NCHAR(10), NCHAR(13), NCHAR(160), NCHAR(11), NCHAR(12));
                """);

            await context.Database.MigrateAsync();
            var profile = await context.OwnerCompanyProfiles.SingleAsync();

            Assert.Equal("Acme\tCo", profile.CompanyName);
            Assert.Equal("1 Main Street", profile.AddressLine1);
            Assert.Null(profile.AddressLine2);
            Assert.Equal("Bangkok", profile.CityProvinceState);
            Assert.Equal("10110", profile.PostalCode);
            Assert.Equal("Thailand", profile.Country);
            Assert.Equal("TAX-1", profile.TaxId);
            Assert.Null(profile.Phone);
            Assert.Null(profile.Email);
            Assert.Null(profile.Website);
            Assert.Null(profile.LogoReference);
            Assert.Null(profile.RegistrationNumber);
        }
        finally
        {
            await SqlServerIntegrationTestDatabase.Delete(databaseName);
        }
    }

    [Fact]
    public async Task Migration_fails_with_actionable_error_for_invalid_required_data()
    {
        var databaseName = SqlServerIntegrationTestDatabase.CreateDatabaseName();

        try
        {
            await using var context = SqlServerIntegrationTestDatabase.CreateContext(databaseName);
            await context.Database.MigrateAsync(PreviousMigration);
            await context.Database.ExecuteSqlRawAsync("""
                INSERT INTO [OwnerCompanyProfiles]
                    ([Id], [CompanyName], [AddressLine1], [CityProvinceState], [PostalCode], [Country])
                VALUES (NEWID(), NCHAR(9) + NCHAR(13) + NCHAR(160), N'Address', N'Bangkok', N'10110', N'Thailand');
                """);

            var exception = await Assert.ThrowsAsync<SqlException>(() => context.Database.MigrateAsync());

            Assert.Contains("CompanyName", exception.Message);
            Assert.Contains("Remove blank values", exception.Message);
        }
        finally
        {
            await SqlServerIntegrationTestDatabase.Delete(databaseName);
        }
    }

    [Fact]
    public async Task Migration_fails_without_truncating_overlength_data()
    {
        var databaseName = SqlServerIntegrationTestDatabase.CreateDatabaseName();
        var value = new string('x', 201);

        try
        {
            await using var context = SqlServerIntegrationTestDatabase.CreateContext(databaseName);
            await context.Database.MigrateAsync(PreviousMigration);
            await context.Database.ExecuteSqlRawAsync(
                "ALTER TABLE [OwnerCompanyProfiles] ALTER COLUMN [CompanyName] nvarchar(max) NOT NULL;");
            await context.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO [OwnerCompanyProfiles]
                    ([Id], [CompanyName], [AddressLine1], [CityProvinceState], [PostalCode], [Country])
                VALUES (NEWID(), {value}, N'Address', N'Bangkok', N'10110', N'Thailand');
                """);

            var exception = await Assert.ThrowsAsync<SqlException>(() => context.Database.MigrateAsync());

            Assert.Contains("CompanyName exceeds 200 characters", exception.Message);
            var storedValue = await context.Database.SqlQueryRaw<string>(
                "SELECT [CompanyName] AS [Value] FROM [OwnerCompanyProfiles]").SingleAsync();
            Assert.Equal(value, storedValue);
        }
        finally
        {
            await SqlServerIntegrationTestDatabase.Delete(databaseName);
        }
    }
}

using System.Text;
using BillingManagement.Application.Abstractions.CompanyMedia;
using BillingManagement.Domain;
using BillingManagement.Infrastructure.CompanyMedia;
using Microsoft.EntityFrameworkCore;

namespace BillingManagement.IntegrationTests;

public sealed class DatabaseCompanyMediaStoreTests
{
    [Fact]
    public async Task Store_persists_blob_across_context_instances()
    {
        var databaseName = SqlServerIntegrationTestDatabase.CreateDatabaseName();

        try
        {
            CompanyMediaStoredFile stored;
            await using (var context = SqlServerIntegrationTestDatabase.CreateContext(databaseName))
            {
                await context.Database.MigrateAsync();
                stored = await new DatabaseCompanyMediaStore(context).StoreAsync(Content("company logo"));
            }

            await using var readContext = SqlServerIntegrationTestDatabase.CreateContext(databaseName);
            var file = await new DatabaseCompanyMediaStore(readContext).OpenReadAsync(stored.Key);

            Assert.NotNull(file);
            Assert.Equal(stored.Key, file.Key);
            Assert.Equal(stored.Length, file.Length);
            await using var content = file.Content;
            Assert.Equal("company logo", await new StreamReader(content).ReadToEndAsync());
        }
        finally
        {
            await SqlServerIntegrationTestDatabase.Delete(databaseName);
        }
    }

    [Fact]
    public async Task Replace_and_delete_preserve_store_contract()
    {
        var databaseName = SqlServerIntegrationTestDatabase.CreateDatabaseName();

        try
        {
            await using var context = SqlServerIntegrationTestDatabase.CreateContext(databaseName);
            await context.Database.MigrateAsync();
            var store = new DatabaseCompanyMediaStore(context);
            var stored = await store.StoreAsync(Content("original"));

            var replaced = await store.ReplaceAsync(stored.Key, Content("replacement"));
            var file = await store.OpenReadAsync(stored.Key);
            var deleted = await store.DeleteAsync(stored.Key);

            Assert.Equal(stored.Key, replaced.Key);
            Assert.Equal(11, replaced.Length);
            Assert.NotNull(file);
            await using var content = file.Content;
            Assert.Equal("replacement", await new StreamReader(content).ReadToEndAsync());
            Assert.True(deleted);
            Assert.Null(await store.OpenReadAsync(stored.Key));
            Assert.False(await store.DeleteAsync(stored.Key));
        }
        finally
        {
            await SqlServerIntegrationTestDatabase.Delete(databaseName);
        }
    }

    [Fact]
    public async Task Migration_maps_media_content_to_sql_server_blob()
    {
        var databaseName = SqlServerIntegrationTestDatabase.CreateDatabaseName();

        try
        {
            await using var context = SqlServerIntegrationTestDatabase.CreateContext(databaseName);
            await context.Database.MigrateAsync();
            var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = 'CompanyMedia' AND COLUMN_NAME = 'Content'
                """;
            await using var reader = await command.ExecuteReaderAsync();

            Assert.True(await reader.ReadAsync());
            Assert.Equal("varbinary", reader.GetString(0));
            Assert.Equal(-1, reader.GetInt32(1));
        }
        finally
        {
            await SqlServerIntegrationTestDatabase.Delete(databaseName);
        }
    }

    [Fact]
    public async Task Legacy_importer_moves_referenced_files_into_database_storage()
    {
        var databaseName = SqlServerIntegrationTestDatabase.CreateDatabaseName();
        var rootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        try
        {
            await using var context = SqlServerIntegrationTestDatabase.CreateContext(databaseName);
            await context.Database.MigrateAsync();
            var key = CompanyMediaStorageKey.Create();
            var profile = OwnerCompanyProfile.Create(
                "Acme", "1 Main Street", null, "Bangkok", "10110", "Thailand",
                null, null, null, null, null, null, key.Value, "image/png");
            context.OwnerCompanyProfiles.Add(profile);
            await context.SaveChangesAsync();
            Directory.CreateDirectory(rootPath);
            await File.WriteAllTextAsync(Path.Combine(rootPath, key.Value), "legacy image");

            var imported = await new LegacyCompanyMediaImporter(context).ImportAsync(rootPath);
            var file = await new DatabaseCompanyMediaStore(context).OpenReadAsync(key);

            Assert.Equal(1, imported);
            Assert.NotNull(file);
            await using var content = file.Content;
            Assert.Equal("legacy image", await new StreamReader(content).ReadToEndAsync());
        }
        finally
        {
            if (Directory.Exists(rootPath))
            {
                Directory.Delete(rootPath, recursive: true);
            }

            await SqlServerIntegrationTestDatabase.Delete(databaseName);
        }
    }

    private static MemoryStream Content(string value)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(value));
    }
}

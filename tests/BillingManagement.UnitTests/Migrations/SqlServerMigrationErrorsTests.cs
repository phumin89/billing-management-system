using BillingManagement.Migrator;

namespace BillingManagement.UnitTests.Migrations;

public sealed class SqlServerMigrationErrorsTests
{
    [Fact]
    public void HasDatabaseAlreadyExistsErrorReturnsTrueForSqlServer1801()
    {
        bool result = SqlServerMigrationErrors.HasDatabaseAlreadyExistsError([1801]);

        Assert.True(result);
    }

    [Fact]
    public void HasDatabaseAlreadyExistsErrorReturnsFalseForOtherSqlServerErrors()
    {
        bool result = SqlServerMigrationErrors.HasDatabaseAlreadyExistsError([4060]);

        Assert.False(result);
    }
}

using Microsoft.Data.SqlClient;

namespace BillingManagement.Migrator;

public static class SqlServerMigrationErrors
{
    public static bool IsDatabaseAlreadyExists(SqlException exception)
    {
        var errorNumbers = exception.Errors
            .Cast<SqlError>()
            .Select(error => error.Number);

        return HasDatabaseAlreadyExistsError(errorNumbers);
    }

    public static bool HasDatabaseAlreadyExistsError(IEnumerable<int> errorNumbers)
    {
        return errorNumbers.Contains(1801);
    }
}

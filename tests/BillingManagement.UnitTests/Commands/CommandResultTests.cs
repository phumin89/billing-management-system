using System.Reflection;
using BillingManagement.Application.Abstractions.Results;

namespace BillingManagement.UnitTests.Commands;

public sealed class CommandResultTests
{
    [Fact]
    public void Public_contract_contains_only_success_and_errors()
    {
        var properties = typeof(CommandResult)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Select(property => property.Name)
            .Order()
            .ToArray();

        Assert.Equal([nameof(CommandResult.Errors), nameof(CommandResult.Success)], properties);
    }
}

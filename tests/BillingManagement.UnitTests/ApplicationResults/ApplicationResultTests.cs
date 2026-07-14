using BillingManagement.Application.Abstractions.Results;

namespace BillingManagement.UnitTests.ApplicationResults;

public sealed class ApplicationResultTests
{
    [Fact]
    public void Success_contains_value_only()
    {
        var result = ApplicationResult<string>.Success("saved");

        Assert.True(result.IsSuccess);
        Assert.Equal("saved", result.Value);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_contains_error_only()
    {
        var error = ApplicationError.Conflict("conflict", "Resource already exists.");

        var result = ApplicationResult<string>.Failure(error);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Same(error, result.Error);
    }
}

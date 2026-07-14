namespace BillingManagement.Application.Abstractions.Results;

public sealed class ApplicationResult<TResult>
{
    private ApplicationResult(TResult? value, ApplicationError? error)
    {
        this.Value = value;
        this.Error = error;
    }

    public bool IsSuccess => this.Error is null;

    public TResult? Value { get; }

    public ApplicationError? Error { get; }

    public static ApplicationResult<TResult> Success(TResult value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new ApplicationResult<TResult>(value, null);
    }

    public static ApplicationResult<TResult> Failure(ApplicationError error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new ApplicationResult<TResult>(default, error);
    }
}

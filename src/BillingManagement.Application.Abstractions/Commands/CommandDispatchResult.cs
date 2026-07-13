namespace BillingManagement.Application.Abstractions.Commands;

public sealed record CommandDispatchResult<TResult>(
    bool IsValid,
    TResult? Result,
    IReadOnlyDictionary<string, string[]> ValidationErrors)
{
    public static CommandDispatchResult<TResult> Success(TResult result) =>
        new(true, result, new Dictionary<string, string[]>());

    public static CommandDispatchResult<TResult> Invalid(
        IReadOnlyDictionary<string, string[]> validationErrors) =>
        new(false, default, validationErrors);
}

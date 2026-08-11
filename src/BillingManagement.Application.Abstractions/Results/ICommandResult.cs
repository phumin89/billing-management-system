namespace BillingManagement.Application.Abstractions.Results;

public interface ICommandResult
{
    bool Success { get; }

    IReadOnlyDictionary<CommandErrorType, IReadOnlyList<string>> Errors { get; }
}

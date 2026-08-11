namespace BillingManagement.Application.Abstractions.Results;

public sealed class CommandResult : ICommandResult
{
    private CommandResult(IReadOnlyDictionary<CommandErrorType, IReadOnlyList<string>> errors)
    {
        this.Errors = errors;
    }

    public bool Success => this.Errors.Count == 0;

    public IReadOnlyDictionary<CommandErrorType, IReadOnlyList<string>> Errors { get; }

    public static CommandResult Succeeded()
    {
        return new CommandResult(
            new Dictionary<CommandErrorType, IReadOnlyList<string>>());
    }

    public static CommandResult Failure(
        CommandErrorType type,
        params string[] messages)
    {
        return new CommandResult(
            new Dictionary<CommandErrorType, IReadOnlyList<string>>
            {
                [type] = messages
            });
    }
}

using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.Results;
using Mediator;

namespace BillingManagement.Application.Handlers.Validation;

public sealed class CommandValidationBehavior<TCommand, TResponse>(
    IEnumerable<ICommandValidator<TCommand>> validators)
    : IPipelineBehavior<TCommand, TResponse>
    where TCommand : notnull, BillingManagement.Application.Abstractions.Commands.ICommand
    where TResponse : ICommandResult
{
    public ValueTask<TResponse> Handle(
        TCommand command,
        MessageHandlerDelegate<TCommand, TResponse> next,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        foreach (var validator in validators)
        {
            foreach (var error in validator.Validate(command))
            {
                foreach (var message in error.Value)
                {
                    AddError(errors, error.Key, message);
                }
            }
        }

        if (errors.Count == 0)
        {
            return next(command, cancellationToken);
        }

        var messages = errors.Values.SelectMany(fieldErrors => fieldErrors).ToArray();
        var result = CommandResult.Failure(CommandErrorType.Validation, messages);
        return ValueTask.FromResult((TResponse)(object)result);
    }

    private static void AddError(
        Dictionary<string, string[]> errors,
        string fieldName,
        string message)
    {
        if (!errors.TryGetValue(fieldName, out var existingErrors))
        {
            errors[fieldName] = [message];
            return;
        }

        errors[fieldName] = [.. existingErrors, message];
    }
}

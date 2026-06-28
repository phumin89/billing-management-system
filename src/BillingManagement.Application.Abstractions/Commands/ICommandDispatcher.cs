namespace BillingManagement.Application.Abstractions.Commands;

public interface ICommandDispatcher
{
    Task<TResult> Send<TCommand, TResult>(
        TCommand command,
        CancellationToken cancellationToken = default);
}

namespace BillingManagement.Application.Abstractions.Commands;

public interface ICommandHandler<in TCommand, TResult>
{
    Task<TResult> Handle(
        TCommand command,
        CancellationToken cancellationToken = default);
}

using BillingManagement.Application.Abstractions.Results;

namespace BillingManagement.Application.Abstractions.Commands;

public interface ICommandDispatcher
{
    Task<ApplicationResult<TResult>> Send<TCommand, TResult>(
        TCommand command,
        CancellationToken cancellationToken = default);
}

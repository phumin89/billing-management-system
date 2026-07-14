using BillingManagement.Application.Abstractions.Results;

namespace BillingManagement.Application.Abstractions.Commands;

public interface ICommandHandler<in TCommand, TResult>
{
    Task<ApplicationResult<TResult>> Handle(
        TCommand command,
        CancellationToken cancellationToken = default);
}

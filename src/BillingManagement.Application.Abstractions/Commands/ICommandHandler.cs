using BillingManagement.Application.Abstractions.Results;

namespace BillingManagement.Application.Abstractions.Commands;

public interface ICommandHandler<in TCommand>
    : Mediator.ICommandHandler<TCommand, CommandResult>
    where TCommand : ICommand;

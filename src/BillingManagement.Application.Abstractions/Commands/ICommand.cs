using BillingManagement.Application.Abstractions.Results;

namespace BillingManagement.Application.Abstractions.Commands;

public interface ICommand : Mediator.ICommand<CommandResult>;

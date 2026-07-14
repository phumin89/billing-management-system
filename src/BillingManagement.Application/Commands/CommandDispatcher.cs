using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.Results;
using BillingManagement.Application.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace BillingManagement.Application.Commands;

public sealed class CommandDispatcher(IServiceProvider services) : ICommandDispatcher
{
    public async Task<ApplicationResult<TResult>> Send<TCommand, TResult>(
        TCommand command,
        CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>();
        foreach (var validator in services.GetServices<ICommandValidator<TCommand>>())
        {
            foreach (var error in validator.Validate(command))
            {
                foreach (var message in error.Value)
                {
                    errors.AddError(error.Key, message);
                }
            }
        }

        if (errors.Count > 0)
        {
            return ApplicationResult<TResult>.Failure(ApplicationError.Validation(
                "validation_failed",
                "One or more validation errors occurred.",
                errors));
        }

        var handler = services.GetRequiredService<ICommandHandler<TCommand, TResult>>();
        return await handler.Handle(command, cancellationToken);
    }
}

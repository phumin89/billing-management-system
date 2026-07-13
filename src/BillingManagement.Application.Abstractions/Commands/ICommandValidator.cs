namespace BillingManagement.Application.Abstractions.Commands;

public interface ICommandValidator<in TCommand>
{
    IReadOnlyDictionary<string, string[]> Validate(TCommand command);
}

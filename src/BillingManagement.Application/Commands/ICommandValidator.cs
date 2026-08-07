using System.ComponentModel.DataAnnotations;

namespace BillingManagement.Application.Commands;

public interface ICommandValidator<TCommand>
{
    IDictionary<string, string[]> Validate(TCommand command);
}
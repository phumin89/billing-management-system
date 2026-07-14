using System.ComponentModel.DataAnnotations;
using System.Reflection;
using BillingManagement.Application.Abstractions.Commands;

namespace BillingManagement.Application.Validation;

public sealed class AnnotationCommandValidator<TCommand> : ICommandValidator<TCommand>
{
    public IReadOnlyDictionary<string, string[]> Validate(TCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var errors = new Dictionary<string, string[]>();
        foreach (var property in typeof(TCommand).GetProperties())
        {
            var value = property.GetValue(command);
            var validationValue = value is string text ? text.Trim() : value;
            var context = new ValidationContext(command)
            {
                MemberName = property.Name
            };

            foreach (var attribute in property.GetCustomAttributes<ValidationAttribute>())
            {
                if (attribute is not RequiredTextAttribute && IsAbsent(value))
                {
                    continue;
                }

                var result = attribute.GetValidationResult(validationValue, context);
                if (result is null)
                {
                    continue;
                }

                errors.AddError(
                    property.Name,
                    result.ErrorMessage ?? throw new InvalidOperationException("Validation error message is required."));
            }
        }

        return errors;
    }

    private static bool IsAbsent(object? value) =>
        value is null || value is string text && string.IsNullOrWhiteSpace(text);
}

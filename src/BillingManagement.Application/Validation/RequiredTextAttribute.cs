using System.ComponentModel.DataAnnotations;

namespace BillingManagement.Application.Validation;

[AttributeUsage(AttributeTargets.Property)]
public sealed class RequiredTextAttribute(string errorMessage)
    : ValidationAttribute(errorMessage)
{
    public override bool IsValid(object? value) =>
        value is string text && !string.IsNullOrWhiteSpace(text);
}

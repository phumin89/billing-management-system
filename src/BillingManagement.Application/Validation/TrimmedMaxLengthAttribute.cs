using System.ComponentModel.DataAnnotations;

namespace BillingManagement.Application.Validation;

[AttributeUsage(AttributeTargets.Property)]
public sealed class TrimmedMaxLengthAttribute : ValidationAttribute
{
    public TrimmedMaxLengthAttribute(int maximumLength)
        : base($"Must not exceed {maximumLength} characters.")
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maximumLength);
        this.MaximumLength = maximumLength;
    }

    public int MaximumLength { get; }

    public override bool IsValid(object? value) =>
        value is null || value is string text && text.Trim().Length <= this.MaximumLength;
}

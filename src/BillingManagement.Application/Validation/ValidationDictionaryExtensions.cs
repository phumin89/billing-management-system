namespace BillingManagement.Application.Validation;

public static class ValidationDictionaryExtensions
{
    public static void AddRequired(
        this Dictionary<string, string[]> errors,
        string fieldName,
        string? value,
        string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.AddError(fieldName, message);
        }
    }

    public static void AddMaxLength(
        this Dictionary<string, string[]> errors,
        string fieldName,
        string? value,
        int maxLength)
    {
        if (value?.Trim().Length > maxLength)
        {
            errors.AddError(fieldName, $"Must not exceed {maxLength} characters.");
        }
    }

    public static void AddEmail(
        this Dictionary<string, string[]> errors,
        string fieldName,
        string? value,
        string message = "Email format is invalid.")
    {
        if (!string.IsNullOrWhiteSpace(value) &&
            !value.Contains('@', StringComparison.Ordinal))
        {
            errors.AddError(fieldName, message);
        }
    }

    internal static void AddError(
        this Dictionary<string, string[]> errors,
        string fieldName,
        string message)
    {
        if (!errors.TryGetValue(fieldName, out var existingErrors))
        {
            errors[fieldName] = [message];
            return;
        }

        errors[fieldName] = [.. existingErrors, message];
    }
}

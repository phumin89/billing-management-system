namespace BillingManagement.Application.Validation;

public static class ValidationDictionaryExtensions
{
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

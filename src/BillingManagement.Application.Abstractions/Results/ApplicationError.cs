using System.Collections.ObjectModel;

namespace BillingManagement.Application.Abstractions.Results;

public sealed class ApplicationError
{
    private ApplicationError(
        ApplicationErrorKind kind,
        string code,
        string message,
        IReadOnlyDictionary<string, string[]>? validationErrors = null)
    {
        this.Kind = kind;
        this.Code = code;
        this.Message = message;
        this.ValidationErrors = validationErrors;
    }

    public ApplicationErrorKind Kind { get; }

    public string Code { get; }

    public string Message { get; }

    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; }

    public static ApplicationError Validation(
        string code,
        string message,
        IReadOnlyDictionary<string, string[]> validationErrors)
    {
        ValidateText(code, message);
        ArgumentNullException.ThrowIfNull(validationErrors);
        if (validationErrors.Count == 0)
        {
            throw new ArgumentException("Validation errors are required.", nameof(validationErrors));
        }

        var copiedErrors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        foreach (var error in validationErrors)
        {
            if (string.IsNullOrWhiteSpace(error.Key))
            {
                throw new ArgumentException("Validation field names are required.", nameof(validationErrors));
            }

            if (error.Value is null || error.Value.Length == 0 || error.Value.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Validation messages are required.", nameof(validationErrors));
            }

            copiedErrors.Add(error.Key, [.. error.Value]);
        }

        return new ApplicationError(
            ApplicationErrorKind.Validation,
            code,
            message,
            new ReadOnlyDictionary<string, string[]>(copiedErrors));
    }

    public static ApplicationError NotFound(string code, string message) =>
        Create(ApplicationErrorKind.NotFound, code, message);

    public static ApplicationError Conflict(string code, string message) =>
        Create(ApplicationErrorKind.Conflict, code, message);

    public static ApplicationError Forbidden(string code, string message) =>
        Create(ApplicationErrorKind.Forbidden, code, message);

    public static ApplicationError Failure(string code, string message) =>
        Create(ApplicationErrorKind.Failure, code, message);

    private static ApplicationError Create(ApplicationErrorKind kind, string code, string message)
    {
        ValidateText(code, message);
        return new ApplicationError(kind, code, message);
    }

    private static void ValidateText(string code, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
    }
}

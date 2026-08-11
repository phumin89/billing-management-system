using BillingManagement.Application.Abstractions.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BillingManagement.Api;

public static class ApplicationErrorMappingExtensions
{
    public static ActionResult ToProblemDetails(
        this ControllerBase controller,
        ICommandResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.Success || result.Errors.Count == 0)
        {
            throw new ArgumentException("A failed command result with at least one error is required.", nameof(result));
        }

        var error = result.Errors.First();
        if (error.Key == CommandErrorType.Validation)
        {
            var modelState = new ModelStateDictionary();
            foreach (var message in error.Value)
            {
                modelState.AddModelError("general", message);
            }

            return controller.ValidationProblem(
                detail: "One or more validation errors occurred.",
                statusCode: StatusCodes.Status400BadRequest,
                modelStateDictionary: modelState,
                extensions: new Dictionary<string, object?> { ["code"] = CommandCode(error.Key) });
        }

        var statusCode = error.Key switch
        {
            CommandErrorType.NotFound => StatusCodes.Status404NotFound,
            CommandErrorType.Conflict => StatusCodes.Status409Conflict,
            CommandErrorType.Forbidden => StatusCodes.Status403Forbidden,
            CommandErrorType.Failure => StatusCodes.Status400BadRequest,
            _ => throw new ArgumentOutOfRangeException(nameof(result), error.Key, "Unsupported command error type.")
        };

        return controller.Problem(
            detail: string.Join(" ", error.Value),
            statusCode: statusCode,
            extensions: new Dictionary<string, object?> { ["code"] = CommandCode(error.Key) });
    }

    public static ActionResult ToProblemDetails(
        this ControllerBase controller,
        ApplicationError error)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(error);

        if (error.Kind == ApplicationErrorKind.Validation)
        {
            return controller.ToValidationProblemDetails(error);
        }

        var statusCode = error.Kind switch
        {
            ApplicationErrorKind.NotFound => StatusCodes.Status404NotFound,
            ApplicationErrorKind.Conflict => StatusCodes.Status409Conflict,
            ApplicationErrorKind.Forbidden => StatusCodes.Status403Forbidden,
            ApplicationErrorKind.Failure => StatusCodes.Status400BadRequest,
            _ => throw new ArgumentOutOfRangeException(nameof(error), error.Kind, "Unsupported application error kind.")
        };

        return controller.Problem(
            detail: error.Message,
            statusCode: statusCode,
            extensions: CodeExtension(error));
    }

    private static ActionResult ToValidationProblemDetails(
        this ControllerBase controller,
        ApplicationError error)
    {
        var modelState = new ModelStateDictionary();
        foreach (var field in error.ValidationErrors!)
        {
            foreach (var message in field.Value)
            {
                modelState.AddModelError(field.Key, message);
            }
        }

        var result = controller.ValidationProblem(
            detail: error.Message,
            statusCode: StatusCodes.Status400BadRequest,
            modelStateDictionary: modelState,
            extensions: CodeExtension(error));

        if (result is not ObjectResult { Value: ValidationProblemDetails problemDetails })
        {
            throw new InvalidOperationException("ValidationProblem did not return ValidationProblemDetails.");
        }

        problemDetails.Errors = error.ValidationErrors.ToDictionary(
            field => field.Key,
            field => field.Value,
            StringComparer.Ordinal);
        return result;
    }

    private static Dictionary<string, object?> CodeExtension(ApplicationError error) =>
        new(StringComparer.Ordinal)
        {
            ["code"] = error.Code
        };

    private static string CommandCode(CommandErrorType errorType) => errorType switch
    {
        CommandErrorType.Validation => "validation_failed",
        CommandErrorType.NotFound => "not_found",
        CommandErrorType.Conflict => "conflict",
        CommandErrorType.Forbidden => "forbidden",
        CommandErrorType.Failure => "failure",
        _ => throw new ArgumentOutOfRangeException(nameof(errorType), errorType, "Unsupported command error type.")
    };
}

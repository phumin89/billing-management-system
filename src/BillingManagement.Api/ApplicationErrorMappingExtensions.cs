using BillingManagement.Application.Abstractions.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BillingManagement.Api;

public static class ApplicationErrorMappingExtensions
{
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
}

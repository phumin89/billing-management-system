namespace BillingManagement.Application.Abstractions.Results;

public enum ApplicationErrorKind
{
    Validation,
    NotFound,
    Conflict,
    Forbidden,
    Failure
}

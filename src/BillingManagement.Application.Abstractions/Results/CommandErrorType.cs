namespace BillingManagement.Application.Abstractions.Results;

public enum CommandErrorType
{
    Validation,
    NotFound,
    Conflict,
    Forbidden,
    Failure
}

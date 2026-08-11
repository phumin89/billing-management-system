namespace BillingManagement.Application.Abstractions.Queries;

public sealed record PageRequest(int PageNumber = 1, int PageSize = 100);

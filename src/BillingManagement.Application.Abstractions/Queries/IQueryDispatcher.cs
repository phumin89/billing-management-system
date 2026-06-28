namespace BillingManagement.Application.Abstractions.Queries;

public interface IQueryDispatcher
{
    Task<TResult> Send<TQuery, TResult>(
        TQuery query,
        CancellationToken cancellationToken = default);
}

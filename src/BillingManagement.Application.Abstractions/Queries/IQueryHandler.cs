namespace BillingManagement.Application.Abstractions.Queries;

public interface IQueryHandler<in TQuery, TResult>
{
    Task<TResult> Handle(
        TQuery query,
        CancellationToken cancellationToken = default);
}

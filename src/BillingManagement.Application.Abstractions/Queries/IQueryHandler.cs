using BillingManagement.Application.Abstractions.Results;

namespace BillingManagement.Application.Abstractions.Queries;

public interface IQueryHandler<in TQuery, TResult>
    : Mediator.IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
    where TResult : IQueryResult;

using BillingManagement.Application.Abstractions.Results;

namespace BillingManagement.Application.Abstractions.Queries;

public interface IQuery<out TResult> : Mediator.IQuery<TResult>
    where TResult : IQueryResult;

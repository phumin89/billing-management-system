using BillingManagement.Application.Abstractions.Queries;

namespace BillingManagement.Application.Customers.GetCustomer;

public sealed record GetCustomerQuery(Guid Id) : IQuery<GetCustomerResult>;

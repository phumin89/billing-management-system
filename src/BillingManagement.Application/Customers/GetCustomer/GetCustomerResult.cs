using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Abstractions.Results;

namespace BillingManagement.Application.Customers.GetCustomer;

public sealed record GetCustomerResult(CustomerRecord? Customer) : IQueryResult;

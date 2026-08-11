using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Abstractions.Results;

namespace BillingManagement.Application.Customers.ListCustomers;

public sealed record ListCustomersResult(
    IReadOnlyList<CustomerRecord> Customers,
    int PageNumber,
    int PageSize,
    int TotalCount) : IQueryResult;

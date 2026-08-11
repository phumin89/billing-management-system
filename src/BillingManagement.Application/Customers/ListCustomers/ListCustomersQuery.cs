using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Abstractions.Queries;

namespace BillingManagement.Application.Customers.ListCustomers;

public sealed record ListCustomersQuery(
    string? SearchText = null,
    int PageNumber = 1,
    int PageSize = 100) : IQuery<ListCustomersResult>;

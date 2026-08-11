using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.Results;

namespace BillingManagement.Application.Customers.DeleteCustomer;

public sealed record DeleteCustomerCommand(Guid Id) : ICommand;

using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Customers.CreateCustomer;
using BillingManagement.Contracts.Customers;
using Microsoft.AspNetCore.Mvc;

namespace BillingManagement.Api.Controllers;

[ApiController]
[Route("api/customers")]
public sealed class CustomersController(ICommandDispatcher commandDispatcher) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> Create(
        CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandDispatcher.Send<CreateCustomerCommand, CustomerRecord>(
            new CreateCustomerCommand(
                request.CustomerName ?? string.Empty,
                request.TaxId,
                request.Email,
                request.Phone,
                request.BillingAddressLine1,
                request.BillingAddressLine2,
                request.CityProvinceState,
                request.PostalCode,
                request.Country,
                request.ContactName,
                request.Notes),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return this.ToProblemDetails(result.Error!);
        }

        var response = ToResponse(result.Value!);
        return this.StatusCode(StatusCodes.Status201Created, response);
    }

    private static CustomerResponse ToResponse(CustomerRecord customer) =>
        new()
        {
            Id = customer.Id,
            CustomerName = customer.CustomerName,
            TaxId = customer.TaxId,
            Email = customer.Email,
            Phone = customer.Phone,
            BillingAddressLine1 = customer.BillingAddressLine1,
            BillingAddressLine2 = customer.BillingAddressLine2,
            CityProvinceState = customer.CityProvinceState,
            PostalCode = customer.PostalCode,
            Country = customer.Country,
            ContactName = customer.ContactName,
            Notes = customer.Notes
        };
}

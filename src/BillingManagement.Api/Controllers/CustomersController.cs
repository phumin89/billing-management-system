using System.ComponentModel.DataAnnotations;
using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Customers.CreateCustomer;
using BillingManagement.Application.Customers.DeleteCustomer;
using BillingManagement.Application.Customers.GetCustomer;
using BillingManagement.Application.Customers.ListCustomers;
using BillingManagement.Application.Customers.UpdateCustomer;
using BillingManagement.Contracts.Customers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace BillingManagement.Api.Controllers;

[ApiController]
[Route("api/customers")]
public sealed class CustomersController(
    ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerResponse>>> List(
        [FromQuery] string? searchText,
        [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
        [FromQuery, Range(1, 100)] int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new ListCustomersQuery(searchText, pageNumber, pageSize), cancellationToken);
        this.Response.Headers.Append("X-Page-Number", result.PageNumber.ToString());
        this.Response.Headers.Append("X-Page-Size", result.PageSize.ToString());
        this.Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        return this.Ok(result.Customers.Select(ToResponse).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<CustomerResponse>> Create(
        CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customerId = Guid.NewGuid();
        var result = await sender.Send(
            new CreateCustomerCommand(
                customerId,
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

        if (!result.Success)
        {
            return this.ToProblemDetails(result);
        }

        var customer = await sender.Send(new GetCustomerQuery(customerId), cancellationToken);
        var response = ToResponse(customer.Customer!);
        return this.StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomerResponse>> Update(
        Guid id,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateCustomerCommand(
                id,
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

        if (!result.Success)
        {
            return this.ToProblemDetails(result);
        }

        var customer = await sender.Send(new GetCustomerQuery(id), cancellationToken);
        return this.Ok(ToResponse(customer.Customer!));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeleteCustomerCommand(id), cancellationToken);

        if (!result.Success)
        {
            return this.ToProblemDetails(result);
        }

        return this.NoContent();
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

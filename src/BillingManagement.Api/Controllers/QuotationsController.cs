using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.BillingDocuments.CreateQuotation;
using BillingManagement.Application.BillingDocuments.GetQuotation;
using BillingManagement.Application.BillingDocuments.ListQuotations;
using BillingManagement.Contracts.BillingDocuments;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace BillingManagement.Api.Controllers;

[ApiController]
[Route("api/quotations")]
public sealed class QuotationsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<QuotationResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListQuotationsQuery(), cancellationToken);
        return this.Ok(result.Items.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuotationResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetQuotationQuery(id), cancellationToken);
        return result.Quotation is null ? this.NotFound() : this.Ok(ToResponse(result.Quotation));
    }

    [HttpPost]
    public async Task<ActionResult<QuotationResponse>> Create(CreateQuotationRequest request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var items = request.Items.Select(item => new BillingDocumentItemRecord(item.Description!, item.Quantity, item.UnitPrice, item.TaxRate)).ToList();
        var command = new CreateQuotationCommand(id, request.Number!, request.CustomerId, request.IssueDate, request.ValidUntil, request.Currency!, items);
        var result = await sender.Send(command, cancellationToken);
        if (!result.Success)
        {
            return this.ToProblemDetails(result);
        }

        var created = await sender.Send(new GetQuotationQuery(id), cancellationToken);
        return this.CreatedAtAction(nameof(this.Get), new { id }, ToResponse(created.Quotation!));
    }

    private static QuotationResponse ToResponse(QuotationRecord item)
    {
        return new QuotationResponse(item.Id, item.Number, item.SellerCompanyName, item.SellerAddress, item.SellerTaxId, item.SellerPhone, item.SellerEmail, item.SellerWebsite, item.SellerRegistrationNumber, item.CustomerId, item.CustomerName, item.CustomerAddress, item.CustomerTaxId, item.IssueDate, item.ValidUntil, item.Currency, item.Items.Select(ToResponse).ToList(), item.Subtotal, item.TaxTotal, item.Total);
    }

    private static BillingDocumentItemResponse ToResponse(BillingDocumentItemRecord item)
    {
        var subtotal = decimal.Round(item.Quantity * item.UnitPrice, 2);
        var tax = decimal.Round(subtotal * item.TaxRate / 100, 2);
        return new BillingDocumentItemResponse(item.Description, item.Quantity, item.UnitPrice, item.TaxRate, subtotal, tax, subtotal + tax);
    }
}

using System.ComponentModel.DataAnnotations;
using BillingManagement.Api.BillingDocuments;
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
public sealed class QuotationsController(ISender sender, BillingDocumentPdfRenderer pdfRenderer) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<QuotationResponse>>> List(
        [FromQuery] string? searchText,
        [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new ListQuotationsQuery(searchText, pageNumber, pageSize), cancellationToken);
        this.AddPaginationHeaders(result.PageNumber, result.PageSize, result.TotalCount);
        return this.Ok(result.Items.Select(ToResponse));
    }

    private void AddPaginationHeaders(int pageNumber, int pageSize, int totalCount)
    {
        this.Response.Headers.Append("X-Page-Number", pageNumber.ToString());
        this.Response.Headers.Append("X-Page-Size", pageSize.ToString());
        this.Response.Headers.Append("X-Total-Count", totalCount.ToString());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuotationResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetQuotationQuery(id), cancellationToken);
        return result.Quotation is null ? this.NotFound() : this.Ok(ToResponse(result.Quotation));
    }

    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> DownloadPdf(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetQuotationQuery(id), cancellationToken);
        if (result.Quotation is null)
        {
            return this.NotFound();
        }

        var fileName = $"{result.Quotation.Number}.pdf";
        return this.File(pdfRenderer.Render(result.Quotation), "application/pdf", fileName);
    }

    [HttpPost]
    public async Task<ActionResult<QuotationResponse>> Create(CreateQuotationRequest request, CancellationToken cancellationToken)
    {
        var items = request.Items.Select(item => new BillingDocumentItemRecord(item.Description!, item.Quantity, item.UnitPrice, item.TaxRate)).ToList();
        var command = CreateQuotationCommand.New(request.Number!, request.CustomerId, request.IssueDate, request.ValidUntil, request.Currency!, items);
        var result = await sender.Send(command, cancellationToken);
        if (!result.Success)
        {
            return this.ToProblemDetails(result);
        }

        var created = await sender.Send(new GetQuotationQuery(command.Id), cancellationToken);
        return this.CreatedAtAction(nameof(this.Get), new { id = command.Id }, ToResponse(created.Quotation!));
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

using System.ComponentModel.DataAnnotations;
using BillingManagement.Api.BillingDocuments;
using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.BillingDocuments.CancelInvoice;
using BillingManagement.Application.BillingDocuments.CreateInvoice;
using BillingManagement.Application.BillingDocuments.GetInvoice;
using BillingManagement.Application.BillingDocuments.ListInvoices;
using BillingManagement.Application.BillingDocuments.MarkInvoicePaid;
using BillingManagement.Contracts.BillingDocuments;
using BillingManagement.Domain;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace BillingManagement.Api.Controllers;

[ApiController]
[Route("api/invoices")]
public sealed class InvoicesController(ISender sender, BillingDocumentPdfRenderer pdfRenderer) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InvoiceResponse>>> List(
        [FromQuery] string? searchText,
        [FromQuery] InvoiceStatus? status,
        [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
        [FromQuery, Range(1, 100)] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new ListInvoicesQuery(searchText, status, pageNumber, pageSize), cancellationToken);
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
    public async Task<ActionResult<InvoiceResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetInvoiceQuery(id), cancellationToken);
        return result.Invoice is null ? this.NotFound() : this.Ok(ToResponse(result.Invoice));
    }

    [HttpGet("{id:guid}/pdf")]
    public async Task<IActionResult> DownloadPdf(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetInvoiceQuery(id), cancellationToken);
        if (result.Invoice is null)
        {
            return this.NotFound();
        }

        var fileName = $"{result.Invoice.Number}.pdf";
        return this.File(pdfRenderer.Render(result.Invoice), "application/pdf", fileName);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceResponse>> Create(CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var result = await sender.Send(new CreateInvoiceCommand(id, request.Number!, request.QuotationId, request.IssueDate, request.DueDate), cancellationToken);
        if (!result.Success)
        {
            return this.ToProblemDetails(result);
        }

        var created = await sender.Send(new GetInvoiceQuery(id), cancellationToken);
        return this.CreatedAtAction(nameof(this.Get), new { id }, ToResponse(created.Invoice!));
    }

    [HttpPost("{id:guid}/mark-paid")]
    public async Task<IActionResult> MarkPaid(Guid id, MarkInvoicePaidRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new MarkInvoicePaidCommand(id, request.PaidDate, request.AmountPaid), cancellationToken);
        return result.Success ? this.NoContent() : this.ToProblemDetails(result);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CancelInvoiceCommand(id), cancellationToken);
        return result.Success ? this.NoContent() : this.ToProblemDetails(result);
    }

    private static InvoiceResponse ToResponse(InvoiceRecord item)
    {
        return new InvoiceResponse(item.Id, item.Number, item.SellerCompanyName, item.SellerAddress, item.SellerTaxId, item.SellerPhone, item.SellerEmail, item.SellerWebsite, item.SellerRegistrationNumber, item.QuotationId, item.CustomerId, item.CustomerName, item.CustomerAddress, item.CustomerTaxId, item.IssueDate, item.DueDate, item.Currency, item.Items.Select(ToResponse).ToList(), item.Subtotal, item.TaxTotal, item.Total, item.DisplayStatus(DateOnly.FromDateTime(DateTime.UtcNow)).ToString(), item.PaidDate, item.AmountPaid);
    }

    private static BillingDocumentItemResponse ToResponse(BillingDocumentItemRecord item)
    {
        var subtotal = decimal.Round(item.Quantity * item.UnitPrice, 2);
        var tax = decimal.Round(subtotal * item.TaxRate / 100, 2);
        return new BillingDocumentItemResponse(item.Description, item.Quantity, item.UnitPrice, item.TaxRate, subtotal, tax, subtotal + tax);
    }
}

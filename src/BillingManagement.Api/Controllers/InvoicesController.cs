using BillingManagement.Api.BillingDocuments;
using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.BillingDocuments.CreateInvoice;
using BillingManagement.Application.BillingDocuments.GetInvoice;
using BillingManagement.Application.BillingDocuments.ListInvoices;
using BillingManagement.Contracts.BillingDocuments;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace BillingManagement.Api.Controllers;

[ApiController]
[Route("api/invoices")]
public sealed class InvoicesController(ISender sender, BillingDocumentPdfRenderer pdfRenderer) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InvoiceResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListInvoicesQuery(), cancellationToken);
        return this.Ok(result.Items.Select(ToResponse));
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

    private static InvoiceResponse ToResponse(InvoiceRecord item)
    {
        return new InvoiceResponse(item.Id, item.Number, item.SellerCompanyName, item.SellerAddress, item.SellerTaxId, item.SellerPhone, item.SellerEmail, item.SellerWebsite, item.SellerRegistrationNumber, item.QuotationId, item.CustomerId, item.CustomerName, item.CustomerAddress, item.CustomerTaxId, item.IssueDate, item.DueDate, item.Currency, item.Items.Select(ToResponse).ToList(), item.Subtotal, item.TaxTotal, item.Total);
    }

    private static BillingDocumentItemResponse ToResponse(BillingDocumentItemRecord item)
    {
        var subtotal = decimal.Round(item.Quantity * item.UnitPrice, 2);
        var tax = decimal.Round(subtotal * item.TaxRate / 100, 2);
        return new BillingDocumentItemResponse(item.Description, item.Quantity, item.UnitPrice, item.TaxRate, subtotal, tax, subtotal + tax);
    }
}

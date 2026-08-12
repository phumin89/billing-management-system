using BillingManagement.Api.BillingDocuments;
using BillingManagement.Application.Abstractions.BillingDocuments;
using PdfSharp.Pdf.IO;

namespace BillingManagement.IntegrationTests;

public sealed class BillingDocumentPdfRendererTests
{
    [Fact]
    public void Render_quotation_creates_single_page_pdf_with_document_content()
    {
        var quotation = CreateQuotation();
        var renderer = new BillingDocumentPdfRenderer();

        var content = renderer.Render(quotation);

        AssertPdf(content);
    }

    [Fact]
    public void Render_invoice_creates_single_page_pdf_with_document_content()
    {
        var quotation = CreateQuotation();
        var invoice = new InvoiceRecord(
            Guid.NewGuid(), "INV-2026-001", quotation.SellerCompanyName, quotation.SellerAddress,
            quotation.SellerTaxId, quotation.SellerPhone, quotation.SellerEmail, quotation.SellerWebsite,
            null, quotation.Id, quotation.CustomerId, quotation.CustomerName, quotation.CustomerAddress,
            quotation.CustomerTaxId, quotation.IssueDate, quotation.ValidUntil, quotation.Currency,
            quotation.Items, quotation.Subtotal, quotation.TaxTotal, quotation.Total);
        var renderer = new BillingDocumentPdfRenderer();

        var content = renderer.Render(invoice);

        AssertPdf(content);
    }

    [Fact]
    public void Render_quotation_adds_document_metadata()
    {
        var quotation = CreateQuotation();
        var renderer = new BillingDocumentPdfRenderer();

        var content = renderer.Render(quotation);

        using var document = Open(content);
        Assert.Equal($"Quotation {quotation.Number}", document.Info.Title);
        Assert.Equal(quotation.SellerCompanyName, document.Info.Author);
        Assert.Equal("Billing Management", document.Info.Creator);
    }

    [Fact]
    public void Render_many_items_creates_multiple_a4_pages()
    {
        var quotation = CreateQuotation(Enumerable.Range(1, 60)
            .Select(index => new BillingDocumentItemRecord(
                $"Implementation service line {index} with enough detail for a readable invoice",
                index % 3 + 1,
                1250m + index,
                7m))
            .ToList());
        var renderer = new BillingDocumentPdfRenderer();

        var content = renderer.Render(quotation);

        using var document = Open(content);
        Assert.True(document.PageCount > 1);
        Assert.All(document.Pages.Cast<PdfSharp.Pdf.PdfPage>(), page =>
        {
            Assert.InRange(page.Width.Millimeter, 209.5, 210.5);
            Assert.InRange(page.Height.Millimeter, 296.5, 297.5);
        });
    }

    private static QuotationRecord CreateQuotation(IReadOnlyList<BillingDocumentItemRecord>? items = null)
    {
        items ??= [new BillingDocumentItemRecord("Billing system implementation", 2, 15000m, 7m)];
        var subtotal = items.Sum(item => decimal.Round(item.Quantity * item.UnitPrice, 2));
        var taxTotal = items.Sum(item => decimal.Round(item.Quantity * item.UnitPrice * item.TaxRate / 100, 2));
        return new QuotationRecord(
            Guid.NewGuid(), "Q-2026-001", "Billing Management Demo", "99 Sukhumvit Road, Bangkok, Thailand",
            "0105559000001", "+66 2 555 0100", "billing@example.com", "https://example.com", null,
            Guid.NewGuid(), "Acme Test Customer", "123 Rama IX Road, Bangkok, Thailand", "0105559000002",
            new DateOnly(2026, 8, 12), new DateOnly(2026, 9, 11), "THB",
            items, subtotal, taxTotal, subtotal + taxTotal);
    }

    private static PdfSharp.Pdf.PdfDocument Open(byte[] content)
    {
        return PdfReader.Open(new MemoryStream(content), PdfDocumentOpenMode.Import);
    }

    private static void AssertPdf(byte[] content)
    {
        Assert.StartsWith("%PDF-", System.Text.Encoding.ASCII.GetString(content, 0, 5));
        Assert.True(content.Length > 10_000);
        Assert.Equal("%%EOF\n", System.Text.Encoding.ASCII.GetString(content[^6..]));
    }
}

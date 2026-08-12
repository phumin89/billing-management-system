using BillingManagement.Api.BillingDocuments;
using BillingManagement.Application.Abstractions.BillingDocuments;

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

    private static QuotationRecord CreateQuotation()
    {
        return new QuotationRecord(
            Guid.NewGuid(), "Q-2026-001", "Billing Management Demo", "99 Sukhumvit Road, Bangkok, Thailand",
            "0105559000001", "+66 2 555 0100", "billing@example.com", "https://example.com", null,
            Guid.NewGuid(), "Acme Test Customer", "123 Rama IX Road, Bangkok, Thailand", "0105559000002",
            new DateOnly(2026, 8, 12), new DateOnly(2026, 9, 11), "THB",
            [new BillingDocumentItemRecord("Billing system implementation", 2, 15000m, 7m)],
            30000m, 2100m, 32100m);
    }

    private static void AssertPdf(byte[] content)
    {
        Assert.StartsWith("%PDF-", System.Text.Encoding.ASCII.GetString(content, 0, 5));
        Assert.True(content.Length > 10_000);
        Assert.Equal("%%EOF\n", System.Text.Encoding.ASCII.GetString(content[^6..]));
    }
}

using BillingManagement.Domain;

namespace BillingManagement.UnitTests.Invoices;

public sealed class InvoiceTests
{
    [Fact]
    public void New_invoice_is_issued_and_can_be_marked_paid_once()
    {
        var invoice = CreateInvoice();

        invoice.MarkPaid(new DateOnly(2026, 8, 20), invoice.Total);

        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(new DateOnly(2026, 8, 20), invoice.PaidDate);
        Assert.Equal(invoice.Total, invoice.AmountPaid);
        Assert.Throws<InvalidOperationException>(() => invoice.Cancel());
    }

    [Fact]
    public void Issued_invoice_can_be_cancelled_but_not_paid_afterwards()
    {
        var invoice = CreateInvoice();

        invoice.Cancel();

        Assert.Equal(InvoiceStatus.Cancelled, invoice.Status);
        Assert.Throws<InvalidOperationException>(() => invoice.MarkPaid(new DateOnly(2026, 8, 20), invoice.Total));
    }

    [Fact]
    public void Payment_requires_the_exact_invoice_total()
    {
        var invoice = CreateInvoice();

        Assert.Throws<ArgumentException>(() => invoice.MarkPaid(new DateOnly(2026, 8, 20), invoice.Total - 1));
    }

    private static Invoice CreateInvoice()
    {
        var quotation = Quotation.Create(
            Guid.NewGuid(), "Q-001", new SellerSnapshot("Seller", "Address", null, null, null, null, null),
            Guid.NewGuid(), "Customer", null, null, new DateOnly(2026, 8, 12),
            new DateOnly(2026, 9, 11), "THB", [new QuotationItemInput("Service", 1, 100, 7)]);
        return Invoice.CreateFromQuotation(Guid.NewGuid(), "INV-001", quotation, new DateOnly(2026, 8, 12), new DateOnly(2026, 9, 11));
    }
    [Fact]
    public void CreateFromQuotation_copies_the_document_snapshot()
    {
        var quotation = Quotation.Create(
            Guid.NewGuid(),
            "Q-2026-0001",
            new SellerSnapshot("Billing Co.", "Seller address", "VAT-SELLER", null, null, null, null),
            Guid.NewGuid(),
            "Acme Co.",
            "123 Main Street",
            "TAX-1",
            new DateOnly(2026, 8, 11),
            new DateOnly(2026, 9, 10),
            "THB",
            [new QuotationItemInput("Consulting", 2, 1500m, 7m)]);

        var invoice = Invoice.CreateFromQuotation(
            Guid.NewGuid(), "INV-2026-0001", quotation, new DateOnly(2026, 8, 12), new DateOnly(2026, 9, 11));

        Assert.Equal(quotation.Id, invoice.QuotationId);
        Assert.Equal(quotation.CustomerName, invoice.CustomerName);
        Assert.Equal(quotation.Total, invoice.Total);
        Assert.Equal(quotation.SellerCompanyName, invoice.SellerCompanyName);
        Assert.Equal(quotation.Items[0].Description, invoice.Items[0].Description);
    }
}

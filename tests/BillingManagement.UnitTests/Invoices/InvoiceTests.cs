using BillingManagement.Domain;

namespace BillingManagement.UnitTests.Invoices;

public sealed class InvoiceTests
{
    [Fact]
    public void CreateFromQuotation_copies_the_document_snapshot()
    {
        var quotation = Quotation.Create(
            Guid.NewGuid(),
            "Q-2026-0001",
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
        Assert.Equal(quotation.Items[0].Description, invoice.Items[0].Description);
    }
}

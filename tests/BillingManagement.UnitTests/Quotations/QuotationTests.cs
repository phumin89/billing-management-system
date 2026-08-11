using BillingManagement.Domain;

namespace BillingManagement.UnitTests.Quotations;

public sealed class QuotationTests
{
    [Fact]
    public void Create_snapshots_customer_and_calculates_totals()
    {
        var quotation = Quotation.Create(
            Guid.NewGuid(),
            "Q-2026-0001",
            Seller(),
            Guid.NewGuid(),
            "Acme Co.",
            "123 Main Street",
            "TAX-1",
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            "THB",
            [new QuotationItemInput("Consulting", 2, 1500m, 7m)]);

        Assert.Equal(3000m, quotation.Subtotal);
        Assert.Equal(210m, quotation.TaxTotal);
        Assert.Equal(3210m, quotation.Total);
        Assert.Equal("Acme Co.", quotation.CustomerName);
        Assert.Equal("Billing Co.", quotation.SellerCompanyName);
        Assert.Equal("VAT-SELLER", quotation.SellerTaxId);
    }

    [Fact]
    public void Create_rejects_empty_items()
    {
        Assert.Throws<ArgumentException>(() => Quotation.Create(
            Guid.NewGuid(),
            "Q-2026-0001",
            Seller(),
            Guid.NewGuid(),
            "Acme Co.",
            null,
            null,
            DateOnly.FromDateTime(DateTime.Today),
            DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            "THB",
            []));
    }

    private static SellerSnapshot Seller()
    {
        return new SellerSnapshot(
            "Billing Co.", "1 Seller Street, Bangkok 10110, Thailand", "VAT-SELLER",
            "0100", "billing@example.com", "https://example.com", "REG-1");
    }
}

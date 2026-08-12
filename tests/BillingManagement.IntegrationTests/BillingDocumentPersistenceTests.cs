using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.Queries;
using BillingManagement.Domain;
using BillingManagement.Infrastructure.BillingDocuments;
using Microsoft.EntityFrameworkCore;

namespace BillingManagement.IntegrationTests;

public sealed class BillingDocumentPersistenceTests
{
    [Fact]
    public async Task Dashboard_groups_financial_totals_by_currency()
    {
        var databaseName = SqlServerIntegrationTestDatabase.CreateDatabaseName();

        try
        {
            await using var context = SqlServerIntegrationTestDatabase.CreateContext(databaseName);
            await context.Database.MigrateAsync();
            var store = new BillingDocumentStore(context);
            var thbQuotation = CreateQuotation("Q-THB", "Acme", new DateOnly(2026, 8, 1));
            var usdQuotation = CreateQuotation("Q-USD", "Beta", new DateOnly(2026, 8, 2), "USD");
            await store.AddQuotation(thbQuotation);
            await store.AddQuotation(usdQuotation);
            var overdue = Invoice.CreateFromQuotation(
                Guid.NewGuid(), "INV-THB", thbQuotation,
                new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 10));
            var paid = Invoice.CreateFromQuotation(
                Guid.NewGuid(), "INV-USD", usdQuotation,
                new DateOnly(2026, 8, 2), new DateOnly(2026, 9, 2));
            paid.MarkPaid(new DateOnly(2026, 8, 5), paid.Total);
            await store.AddInvoice(overdue);
            await store.AddInvoice(paid);
            context.ChangeTracker.Clear();

            var dashboard = await store.GetInvoiceDashboard(new DateOnly(2026, 8, 12));

            var outstanding = Assert.Single(dashboard.Outstanding);
            Assert.Equal("THB", outstanding.Currency);
            Assert.Equal(107m, outstanding.Value);
            Assert.Equal("USD", Assert.Single(dashboard.PaidThisMonth).Currency);
            Assert.Equal("INV-USD", dashboard.RecentInvoices[0].Number);
            Assert.Equal("THB", Assert.Single(dashboard.Overdue).Currency);
        }
        finally
        {
            await SqlServerIntegrationTestDatabase.Delete(databaseName);
        }
    }

    [Fact]
    public async Task Search_pages_and_filters_billing_documents()
    {
        var databaseName = SqlServerIntegrationTestDatabase.CreateDatabaseName();

        try
        {
            await using var context = SqlServerIntegrationTestDatabase.CreateContext(databaseName);
            await context.Database.MigrateAsync();
            var store = new BillingDocumentStore(context);
            var acme = CreateQuotation("Q-002", "Acme", new DateOnly(2026, 8, 2));
            var beta = CreateQuotation("Q-001", "Beta", new DateOnly(2026, 8, 1));
            await store.AddQuotation(acme);
            await store.AddQuotation(beta);
            var overdue = Invoice.CreateFromQuotation(
                Guid.NewGuid(), "INV-002", acme, new DateOnly(2026, 8, 2), new DateOnly(2026, 8, 10));
            var paid = Invoice.CreateFromQuotation(
                Guid.NewGuid(), "INV-001", beta, new DateOnly(2026, 8, 1), new DateOnly(2026, 9, 1));
            paid.MarkPaid(new DateOnly(2026, 8, 5), paid.Total);
            await store.AddInvoice(overdue);
            await store.AddInvoice(paid);
            context.ChangeTracker.Clear();

            var quotationPage = await store.SearchQuotations(
                new QuotationSearchCriteria(), new PageRequest(1, 1));
            var matchingQuotation = await store.SearchQuotations(
                new QuotationSearchCriteria("Beta"), new PageRequest());
            var overdueInvoices = await store.SearchInvoices(
                new InvoiceSearchCriteria(Status: InvoiceStatus.Overdue, Today: new DateOnly(2026, 8, 12)),
                new PageRequest());

            Assert.Equal(2, quotationPage.TotalCount);
            Assert.Single(quotationPage.Items);
            Assert.Equal("Q-002", quotationPage.Items[0].Number);
            Assert.Equal("Q-001", Assert.Single(matchingQuotation.Items).Number);
            Assert.Equal("INV-002", Assert.Single(overdueInvoices.Items).Number);
        }
        finally
        {
            await SqlServerIntegrationTestDatabase.Delete(databaseName);
        }
    }

    [Fact]
    public async Task Store_round_trips_quotation_and_invoice_snapshots()
    {
        var databaseName = SqlServerIntegrationTestDatabase.CreateDatabaseName();

        try
        {
            await using var context = SqlServerIntegrationTestDatabase.CreateContext(databaseName);
            await context.Database.MigrateAsync();
            var quotation = Quotation.Create(
                Guid.NewGuid(), "Q-2026-0001",
                new SellerSnapshot("Billing Co.", "Seller address", "VAT-SELLER", null, null, null, null),
                Guid.NewGuid(), "Acme", "Bangkok", "TAX-1",
                new DateOnly(2026, 8, 11), new DateOnly(2026, 9, 10), "THB",
                [new QuotationItemInput("Consulting", 2, 1500m, 7m)]);
            var store = new BillingDocumentStore(context);

            await store.AddQuotation(quotation);
            var persistedQuotation = await store.GetQuotationEntity(quotation.Id);
            var invoice = Invoice.CreateFromQuotation(
                Guid.NewGuid(), "INV-2026-0001", persistedQuotation!,
                new DateOnly(2026, 8, 12), new DateOnly(2026, 9, 11));
            await store.AddInvoice(invoice);
            context.ChangeTracker.Clear();

            var quotationRecord = await store.GetQuotation(quotation.Id);
            var invoiceRecord = await store.GetInvoice(invoice.Id);

            Assert.Equal(3210m, quotationRecord!.Total);
            Assert.Equal("Billing Co.", quotationRecord.SellerCompanyName);
            Assert.Equal("Acme", invoiceRecord!.CustomerName);
            Assert.Equal("Billing Co.", invoiceRecord.SellerCompanyName);
            Assert.Equal(quotation.Id, invoiceRecord.QuotationId);
            Assert.Single(invoiceRecord.Items);
        }
        finally
        {
            await SqlServerIntegrationTestDatabase.Delete(databaseName);
        }
    }

    private static Quotation CreateQuotation(
        string number,
        string customerName,
        DateOnly issueDate,
        string currency = "THB")
    {
        return Quotation.Create(
            Guid.NewGuid(), number,
            new SellerSnapshot("Billing Co.", "Seller address", null, null, null, null, null),
            Guid.NewGuid(), customerName, "Bangkok", null,
            issueDate, issueDate.AddDays(30), currency,
            [new QuotationItemInput("Service", 1, 100m, 7m)]);
    }
}

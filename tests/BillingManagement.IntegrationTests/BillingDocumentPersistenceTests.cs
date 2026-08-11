using BillingManagement.Domain;
using BillingManagement.Infrastructure.BillingDocuments;
using Microsoft.EntityFrameworkCore;

namespace BillingManagement.IntegrationTests;

public sealed class BillingDocumentPersistenceTests
{
    [Fact]
    public async Task Store_round_trips_quotation_and_invoice_snapshots()
    {
        var databaseName = SqlServerIntegrationTestDatabase.CreateDatabaseName();

        try
        {
            await using var context = SqlServerIntegrationTestDatabase.CreateContext(databaseName);
            await context.Database.MigrateAsync();
            var quotation = Quotation.Create(
                Guid.NewGuid(), "Q-2026-0001", Guid.NewGuid(), "Acme", "Bangkok", "TAX-1",
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
            Assert.Equal("Acme", invoiceRecord!.CustomerName);
            Assert.Equal(quotation.Id, invoiceRecord.QuotationId);
            Assert.Single(invoiceRecord.Items);
        }
        finally
        {
            await SqlServerIntegrationTestDatabase.Delete(databaseName);
        }
    }
}

using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.BillingDocuments.CreateInvoice;
using BillingManagement.Application.BillingDocuments.CreateQuotation;
using BillingManagement.Application.Validation;

namespace BillingManagement.UnitTests.BillingDocuments;

public sealed class BillingDocumentCommandValidationTests
{
    [Fact]
    public void Quotation_requires_number_currency_and_line_item()
    {
        var command = new CreateQuotationCommand(
            Guid.NewGuid(), " ", Guid.NewGuid(), new DateOnly(2026, 8, 12),
            new DateOnly(2026, 9, 11), "US", []);

        var errors = new AnnotationCommandValidator<CreateQuotationCommand>().Validate(command);

        Assert.Contains(nameof(command.Number), errors.Keys);
        Assert.Contains(nameof(command.Currency), errors.Keys);
        Assert.Contains(nameof(command.Items), errors.Keys);
    }

    [Fact]
    public void Invoice_requires_number()
    {
        var command = new CreateInvoiceCommand(
            Guid.NewGuid(), " ", Guid.NewGuid(), new DateOnly(2026, 8, 12), new DateOnly(2026, 9, 11));

        var errors = new AnnotationCommandValidator<CreateInvoiceCommand>().Validate(command);

        Assert.Contains(nameof(command.Number), errors.Keys);
    }
}

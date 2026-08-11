using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Abstractions.Results;
using BillingManagement.Application.BillingDocuments.CreateQuotation;
using BillingManagement.Domain;

namespace BillingManagement.Application.BillingDocuments.CreateQuotation;

public sealed class CreateQuotationHandler(ICustomerQueries customers, IBillingDocumentStore store) : ICommandHandler<CreateQuotationCommand>
{
    public async ValueTask<CommandResult> Handle(CreateQuotationCommand command, CancellationToken cancellationToken = default)
    {
        if (await store.QuotationNumberExists(command.Number.Trim(), cancellationToken))
        {
            return CommandResult.Failure(CommandErrorType.Conflict, "Quotation number already exists.");
        }

        var customer = await customers.GetById(command.CustomerId, cancellationToken);
        if (customer is null)
        {
            return CommandResult.Failure(CommandErrorType.NotFound, "Customer was not found.");
        }

        var address = string.Join(", ", new[] { customer.BillingAddressLine1, customer.BillingAddressLine2, customer.CityProvinceState, customer.PostalCode, customer.Country }.Where(value => !string.IsNullOrWhiteSpace(value)));
        var quotation = Quotation.Create(command.Id, command.Number, customer.Id, customer.CustomerName, address, customer.TaxId, command.IssueDate, command.ValidUntil, command.Currency, command.Items.Select(item => new QuotationItemInput(item.Description, item.Quantity, item.UnitPrice, item.TaxRate)).ToList());
        await store.AddQuotation(quotation, cancellationToken);
        return CommandResult.Succeeded();
    }
}

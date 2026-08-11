using BillingManagement.Application.Abstractions.BillingDocuments;
using BillingManagement.Application.Abstractions.Commands;
using BillingManagement.Application.Abstractions.Customers;
using BillingManagement.Application.Abstractions.OwnerCompanyProfiles;
using BillingManagement.Application.Abstractions.Results;
using BillingManagement.Application.BillingDocuments.CreateQuotation;
using BillingManagement.Domain;

namespace BillingManagement.Application.BillingDocuments.CreateQuotation;

public sealed class CreateQuotationHandler(
    ICustomerQueries customers,
    IOwnerCompanyProfileStore ownerProfiles,
    IBillingDocumentStore store) : ICommandHandler<CreateQuotationCommand>
{
    public async ValueTask<CommandResult> Handle(CreateQuotationCommand command, CancellationToken cancellationToken = default)
    {
        var validationFailure = Validate(command);
        if (validationFailure is not null)
        {
            return validationFailure;
        }

        if (await store.QuotationNumberExists(command.Number.Trim(), cancellationToken))
        {
            return CommandResult.Failure(CommandErrorType.Conflict, "Quotation number already exists.");
        }

        var customer = await customers.GetById(command.CustomerId, cancellationToken);
        if (customer is null)
        {
            return CommandResult.Failure(CommandErrorType.NotFound, "Customer was not found.");
        }

        var owner = await ownerProfiles.GetAsync(cancellationToken);
        if (owner is null)
        {
            return CommandResult.Failure(CommandErrorType.Conflict, "Create the company profile before creating a quotation.");
        }

        var quotation = CreateQuotation(command, customer, owner);
        await store.AddQuotation(quotation, cancellationToken);
        return CommandResult.Succeeded();
    }

    private static CommandResult? Validate(CreateQuotationCommand command)
    {
        if (command.CustomerId == Guid.Empty)
        {
            return CommandResult.Failure(CommandErrorType.Validation, "Customer is required.");
        }

        if (command.ValidUntil < command.IssueDate)
        {
            return CommandResult.Failure(CommandErrorType.Validation, "Valid-until date cannot precede the issue date.");
        }

        return command.Items.Any(item => string.IsNullOrWhiteSpace(item.Description) || item.Quantity <= 0 || item.UnitPrice < 0 || item.TaxRate is < 0 or > 100)
            ? CommandResult.Failure(CommandErrorType.Validation, "Every line item must have a description, positive quantity, non-negative price, and tax from 0 to 100.")
            : null;
    }

    private static Quotation CreateQuotation(
        CreateQuotationCommand command,
        CustomerRecord customer,
        OwnerCompanyProfileRecord owner)
    {
        var seller = new SellerSnapshot(
            owner.CompanyName, JoinAddress(owner.AddressLine1, owner.AddressLine2, owner.City, owner.PostalCode, owner.Country),
            owner.TaxId, owner.Phone, owner.Email, owner.Website, owner.RegistrationNumber);
        var items = command.Items.Select(item =>
            new QuotationItemInput(item.Description, item.Quantity, item.UnitPrice, item.TaxRate)).ToList();
        return Quotation.Create(
            command.Id, command.Number, seller, customer.Id, customer.CustomerName,
            JoinAddress(customer.BillingAddressLine1, customer.BillingAddressLine2, customer.CityProvinceState, customer.PostalCode, customer.Country),
            customer.TaxId, command.IssueDate, command.ValidUntil, command.Currency, items);
    }

    private static string JoinAddress(params string?[] parts)
    {
        return string.Join(", ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
    }
}

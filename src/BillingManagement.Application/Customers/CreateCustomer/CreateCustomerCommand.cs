using System.ComponentModel.DataAnnotations;
using BillingManagement.Application.Validation;
using BillingManagement.Domain;

namespace BillingManagement.Application.Customers.CreateCustomer;

public sealed record CreateCustomerCommand(
    [property: RequiredText("Customer name is required.")]
    [property: TrimmedMaxLength(CustomerConstraints.CustomerNameMaxLength)]
    string CustomerName,
    [property: TrimmedMaxLength(CustomerConstraints.TaxIdMaxLength)]
    string? TaxId,
    [property: TrimmedMaxLength(CustomerConstraints.EmailMaxLength)]
    [property: EmailAddress(ErrorMessage = "Email format is invalid.")]
    string? Email,
    [property: TrimmedMaxLength(CustomerConstraints.PhoneMaxLength)]
    string? Phone,
    [property: TrimmedMaxLength(CustomerConstraints.BillingAddressLine1MaxLength)]
    string? BillingAddressLine1,
    [property: TrimmedMaxLength(CustomerConstraints.BillingAddressLine2MaxLength)]
    string? BillingAddressLine2,
    [property: TrimmedMaxLength(CustomerConstraints.CityProvinceStateMaxLength)]
    string? CityProvinceState,
    [property: TrimmedMaxLength(CustomerConstraints.PostalCodeMaxLength)]
    string? PostalCode,
    [property: TrimmedMaxLength(CustomerConstraints.CountryMaxLength)]
    string? Country,
    [property: TrimmedMaxLength(CustomerConstraints.ContactNameMaxLength)]
    string? ContactName,
    [property: TrimmedMaxLength(CustomerConstraints.NotesMaxLength)]
    string? Notes);

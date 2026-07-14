using System.ComponentModel.DataAnnotations;
using BillingManagement.Application.Validation;
using BillingManagement.Domain;

namespace BillingManagement.Application.OwnerCompanyProfiles.CreateOwnerCompanyProfile;

public sealed record CreateOwnerCompanyProfileCommand(
    [property: RequiredText("Company name is required.")]
    [property: TrimmedMaxLength(OwnerCompanyProfileConstraints.CompanyNameMaxLength)]
    string CompanyName,
    [property: RequiredText("Address line 1 is required.")]
    [property: TrimmedMaxLength(OwnerCompanyProfileConstraints.AddressLine1MaxLength)]
    string AddressLine1,
    [property: TrimmedMaxLength(OwnerCompanyProfileConstraints.AddressLine2MaxLength)]
    string? AddressLine2,
    [property: RequiredText("City / province / state is required.")]
    [property: TrimmedMaxLength(OwnerCompanyProfileConstraints.CityProvinceStateMaxLength)]
    string City,
    [property: RequiredText("Postal code is required.")]
    [property: TrimmedMaxLength(OwnerCompanyProfileConstraints.PostalCodeMaxLength)]
    string PostalCode,
    [property: RequiredText("Country is required.")]
    [property: TrimmedMaxLength(OwnerCompanyProfileConstraints.CountryMaxLength)]
    string Country,
    [property: TrimmedMaxLength(OwnerCompanyProfileConstraints.TaxIdMaxLength)]
    string? TaxId,
    [property: TrimmedMaxLength(OwnerCompanyProfileConstraints.PhoneMaxLength)]
    string? Phone,
    [property: TrimmedMaxLength(OwnerCompanyProfileConstraints.EmailMaxLength)]
    [property: EmailAddress(ErrorMessage = "Email format is invalid.")]
    string? Email,
    [property: TrimmedMaxLength(OwnerCompanyProfileConstraints.WebsiteMaxLength)]
    string? Website,
    [property: TrimmedMaxLength(OwnerCompanyProfileConstraints.LogoReferenceMaxLength)]
    string? LogoReference,
    [property: TrimmedMaxLength(OwnerCompanyProfileConstraints.RegistrationNumberMaxLength)]
    string? RegistrationNumber);

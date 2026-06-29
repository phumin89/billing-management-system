namespace BillingManagement.Domain;

public sealed class OwnerCompanyProfile
{
    private OwnerCompanyProfile()
    {
    }

    private OwnerCompanyProfile(
        Guid id,
        string companyName,
        string addressLine1,
        string? addressLine2,
        string city,
        string postalCode,
        string country,
        string? taxId,
        string? phone,
        string? email,
        string? website,
        string? logoReference,
        string? registrationNumber)
    {
        Id = id;
        CompanyName = companyName;
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        PostalCode = postalCode;
        Country = country;
        TaxId = taxId;
        Phone = phone;
        Email = email;
        Website = website;
        LogoReference = logoReference;
        RegistrationNumber = registrationNumber;
    }

    public Guid Id { get; private set; }

    public string CompanyName { get; private set; } = string.Empty;

    public string AddressLine1 { get; private set; } = string.Empty;

    public string? AddressLine2 { get; private set; }

    public string City { get; private set; } = string.Empty;

    public string PostalCode { get; private set; } = string.Empty;

    public string Country { get; private set; } = string.Empty;

    public string? TaxId { get; private set; }

    public string? Phone { get; private set; }

    public string? Email { get; private set; }

    public string? Website { get; private set; }

    public string? LogoReference { get; private set; }

    public string? RegistrationNumber { get; private set; }

    public static OwnerCompanyProfile Create(
        Guid id,
        string companyName,
        string addressLine1,
        string? addressLine2,
        string city,
        string postalCode,
        string country,
        string? taxId,
        string? phone,
        string? email,
        string? website,
        string? logoReference,
        string? registrationNumber) =>
        new(
            id,
            companyName,
            addressLine1,
            addressLine2,
            city,
            postalCode,
            country,
            taxId,
            phone,
            email,
            website,
            logoReference,
            registrationNumber);
}

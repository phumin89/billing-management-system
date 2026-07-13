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
        string cityProvinceState,
        string postalCode,
        string country,
        string? taxId,
        string? phone,
        string? email,
        string? website,
        string? logoReference,
        string? registrationNumber)
    {
        this.Id = id;
        this.SetValues(
            companyName, addressLine1, addressLine2, cityProvinceState, postalCode,
            country, taxId, phone, email, website, logoReference, registrationNumber);
    }

    public Guid Id { get; private set; }

    public string CompanyName { get; private set; } = string.Empty;

    public string AddressLine1 { get; private set; } = string.Empty;

    public string? AddressLine2 { get; private set; }

    public string CityProvinceState { get; private set; } = string.Empty;

    public string PostalCode { get; private set; } = string.Empty;

    public string Country { get; private set; } = string.Empty;

    public string? TaxId { get; private set; }

    public string? Phone { get; private set; }

    public string? Email { get; private set; }

    public string? Website { get; private set; }

    public string? LogoReference { get; private set; }

    public string? RegistrationNumber { get; private set; }

    public static OwnerCompanyProfile Create(
        string companyName,
        string addressLine1,
        string? addressLine2,
        string cityProvinceState,
        string postalCode,
        string country,
        string? taxId,
        string? phone,
        string? email,
        string? website,
        string? logoReference,
        string? registrationNumber) =>
        new(
            Guid.NewGuid(),
            companyName,
            addressLine1,
            addressLine2,
            cityProvinceState,
            postalCode,
            country,
            taxId,
            phone,
            email,
            website,
            logoReference,
            registrationNumber);

    public static OwnerCompanyProfile Rehydrate(
        Guid id,
        string companyName,
        string addressLine1,
        string? addressLine2,
        string cityProvinceState,
        string postalCode,
        string country,
        string? taxId,
        string? phone,
        string? email,
        string? website,
        string? logoReference,
        string? registrationNumber)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Persisted ID cannot be empty.", nameof(id));
        }

        return new(
            id,
            companyName,
            addressLine1,
            addressLine2,
            cityProvinceState,
            postalCode,
            country,
            taxId,
            phone,
            email,
            website,
            logoReference,
            registrationNumber);
    }

    public void Update(
        string companyName,
        string addressLine1,
        string? addressLine2,
        string cityProvinceState,
        string postalCode,
        string country,
        string? taxId,
        string? phone,
        string? email,
        string? website,
        string? logoReference,
        string? registrationNumber)
    {
        this.SetValues(
            companyName, addressLine1, addressLine2, cityProvinceState, postalCode,
            country, taxId, phone, email, website, logoReference, registrationNumber);
    }

    private void SetValues(
        string companyName,
        string addressLine1,
        string? addressLine2,
        string cityProvinceState,
        string postalCode,
        string country,
        string? taxId,
        string? phone,
        string? email,
        string? website,
        string? logoReference,
        string? registrationNumber)
    {
        var normalizedCompanyName = NormalizeRequired(companyName, OwnerCompanyProfileConstraints.CompanyNameMaxLength, nameof(companyName));
        var normalizedAddressLine1 = NormalizeRequired(addressLine1, OwnerCompanyProfileConstraints.AddressLine1MaxLength, nameof(addressLine1));
        var normalizedAddressLine2 = NormalizeOptional(addressLine2, OwnerCompanyProfileConstraints.AddressLine2MaxLength, nameof(addressLine2));
        var normalizedCityProvinceState = NormalizeRequired(
            cityProvinceState,
            OwnerCompanyProfileConstraints.CityProvinceStateMaxLength,
            nameof(cityProvinceState));
        var normalizedPostalCode = NormalizeRequired(postalCode, OwnerCompanyProfileConstraints.PostalCodeMaxLength, nameof(postalCode));
        var normalizedCountry = NormalizeRequired(country, OwnerCompanyProfileConstraints.CountryMaxLength, nameof(country));
        var normalizedTaxId = NormalizeOptional(taxId, OwnerCompanyProfileConstraints.TaxIdMaxLength, nameof(taxId));
        var normalizedPhone = NormalizeOptional(phone, OwnerCompanyProfileConstraints.PhoneMaxLength, nameof(phone));
        var normalizedEmail = NormalizeOptional(email, OwnerCompanyProfileConstraints.EmailMaxLength, nameof(email));
        var normalizedWebsite = NormalizeOptional(website, OwnerCompanyProfileConstraints.WebsiteMaxLength, nameof(website));
        var normalizedLogoReference = NormalizeOptional(
            logoReference,
            OwnerCompanyProfileConstraints.LogoReferenceMaxLength,
            nameof(logoReference));
        var normalizedRegistrationNumber = NormalizeOptional(
            registrationNumber,
            OwnerCompanyProfileConstraints.RegistrationNumberMaxLength,
            nameof(registrationNumber));

        this.CompanyName = normalizedCompanyName;
        this.AddressLine1 = normalizedAddressLine1;
        this.AddressLine2 = normalizedAddressLine2;
        this.CityProvinceState = normalizedCityProvinceState;
        this.PostalCode = normalizedPostalCode;
        this.Country = normalizedCountry;
        this.TaxId = normalizedTaxId;
        this.Phone = normalizedPhone;
        this.Email = normalizedEmail;
        this.Website = normalizedWebsite;
        this.LogoReference = normalizedLogoReference;
        this.RegistrationNumber = normalizedRegistrationNumber;
    }

    private static string NormalizeRequired(string value, int maxLength, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return NormalizeLength(value.Trim(), maxLength, parameterName);
    }

    private static string? NormalizeOptional(string? value, int maxLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return NormalizeLength(value.Trim(), maxLength, parameterName);
    }

    private static string NormalizeLength(string value, int maxLength, string parameterName)
    {
        if (value.Length > maxLength)
        {
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.", parameterName);
        }

        return value;
    }
}

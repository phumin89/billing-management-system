namespace BillingManagement.Domain;

public sealed class OwnerCompanyProfile
{
    public const int CompanyNameMaxLength = 200;
    public const int AddressLine1MaxLength = 300;
    public const int AddressLine2MaxLength = 300;
    public const int CityProvinceStateMaxLength = 150;
    public const int PostalCodeMaxLength = 50;
    public const int CountryMaxLength = 100;
    public const int TaxIdMaxLength = 100;
    public const int PhoneMaxLength = 100;
    public const int EmailMaxLength = 254;
    public const int WebsiteMaxLength = 300;
    public const int LogoReferenceMaxLength = 500;
    public const int RegistrationNumberMaxLength = 100;
    public const byte SingletonKeyValue = 1;

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

    public byte SingletonKey { get; private set; } = SingletonKeyValue;

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
        string? registrationNumber) =>
        new(
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
        var normalizedCompanyName = NormalizeRequired(companyName, CompanyNameMaxLength, nameof(companyName));
        var normalizedAddressLine1 = NormalizeRequired(addressLine1, AddressLine1MaxLength, nameof(addressLine1));
        var normalizedAddressLine2 = NormalizeOptional(addressLine2, AddressLine2MaxLength, nameof(addressLine2));
        var normalizedCityProvinceState = NormalizeRequired(
            cityProvinceState,
            CityProvinceStateMaxLength,
            nameof(cityProvinceState));
        var normalizedPostalCode = NormalizeRequired(postalCode, PostalCodeMaxLength, nameof(postalCode));
        var normalizedCountry = NormalizeRequired(country, CountryMaxLength, nameof(country));
        var normalizedTaxId = NormalizeOptional(taxId, TaxIdMaxLength, nameof(taxId));
        var normalizedPhone = NormalizeOptional(phone, PhoneMaxLength, nameof(phone));
        var normalizedEmail = NormalizeOptional(email, EmailMaxLength, nameof(email));
        var normalizedWebsite = NormalizeOptional(website, WebsiteMaxLength, nameof(website));
        var normalizedLogoReference = NormalizeOptional(
            logoReference,
            LogoReferenceMaxLength,
            nameof(logoReference));
        var normalizedRegistrationNumber = NormalizeOptional(
            registrationNumber,
            RegistrationNumberMaxLength,
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

namespace BillingManagement.Domain;

public sealed class Customer
{
    private Customer()
    {
    }

    private Customer(
        Guid id,
        string customerName,
        string? taxId,
        string? email,
        string? phone,
        string? billingAddressLine1,
        string? billingAddressLine2,
        string? cityProvinceState,
        string? postalCode,
        string? country,
        string? contactName,
        string? notes)
    {
        this.Id = id;
        this.CustomerName = NormalizeRequired(customerName, CustomerConstraints.CustomerNameMaxLength, nameof(customerName));
        this.TaxId = NormalizeOptional(taxId, CustomerConstraints.TaxIdMaxLength, nameof(taxId));
        this.Email = NormalizeOptional(email, CustomerConstraints.EmailMaxLength, nameof(email));
        this.Phone = NormalizeOptional(phone, CustomerConstraints.PhoneMaxLength, nameof(phone));
        this.BillingAddressLine1 = NormalizeOptional(billingAddressLine1, CustomerConstraints.BillingAddressLine1MaxLength, nameof(billingAddressLine1));
        this.BillingAddressLine2 = NormalizeOptional(billingAddressLine2, CustomerConstraints.BillingAddressLine2MaxLength, nameof(billingAddressLine2));
        this.CityProvinceState = NormalizeOptional(cityProvinceState, CustomerConstraints.CityProvinceStateMaxLength, nameof(cityProvinceState));
        this.PostalCode = NormalizeOptional(postalCode, CustomerConstraints.PostalCodeMaxLength, nameof(postalCode));
        this.Country = NormalizeOptional(country, CustomerConstraints.CountryMaxLength, nameof(country));
        this.ContactName = NormalizeOptional(contactName, CustomerConstraints.ContactNameMaxLength, nameof(contactName));
        this.Notes = NormalizeOptional(notes, CustomerConstraints.NotesMaxLength, nameof(notes));
    }

    public Guid Id { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string? TaxId { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? BillingAddressLine1 { get; private set; }
    public string? BillingAddressLine2 { get; private set; }
    public string? CityProvinceState { get; private set; }
    public string? PostalCode { get; private set; }
    public string? Country { get; private set; }
    public string? ContactName { get; private set; }
    public string? Notes { get; private set; }

    public static Customer Create(
        string customerName,
        string? taxId,
        string? email,
        string? phone,
        string? billingAddressLine1,
        string? billingAddressLine2,
        string? cityProvinceState,
        string? postalCode,
        string? country,
        string? contactName,
        string? notes) =>
        new(Guid.NewGuid(), customerName, taxId, email, phone, billingAddressLine1,
            billingAddressLine2, cityProvinceState, postalCode, country, contactName, notes);

    public static Customer Create(
        Guid id,
        string customerName,
        string? taxId,
        string? email,
        string? phone,
        string? billingAddressLine1,
        string? billingAddressLine2,
        string? cityProvinceState,
        string? postalCode,
        string? country,
        string? contactName,
        string? notes)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("ID cannot be empty.", nameof(id));
        }

        return new Customer(id, customerName, taxId, email, phone, billingAddressLine1,
            billingAddressLine2, cityProvinceState, postalCode, country, contactName, notes);
    }

    public static Customer Rehydrate(
        Guid id,
        string customerName,
        string? taxId,
        string? email,
        string? phone,
        string? billingAddressLine1,
        string? billingAddressLine2,
        string? cityProvinceState,
        string? postalCode,
        string? country,
        string? contactName,
        string? notes)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Persisted ID cannot be empty.", nameof(id));
        }

        return new(id, customerName, taxId, email, phone, billingAddressLine1,
            billingAddressLine2, cityProvinceState, postalCode, country, contactName, notes);
    }

    public void Update(
        string customerName,
        string? taxId,
        string? email,
        string? phone,
        string? billingAddressLine1,
        string? billingAddressLine2,
        string? cityProvinceState,
        string? postalCode,
        string? country,
        string? contactName,
        string? notes)
    {
        this.CustomerName = NormalizeRequired(customerName, CustomerConstraints.CustomerNameMaxLength, nameof(customerName));
        this.TaxId = NormalizeOptional(taxId, CustomerConstraints.TaxIdMaxLength, nameof(taxId));
        this.Email = NormalizeOptional(email, CustomerConstraints.EmailMaxLength, nameof(email));
        this.Phone = NormalizeOptional(phone, CustomerConstraints.PhoneMaxLength, nameof(phone));
        this.BillingAddressLine1 = NormalizeOptional(billingAddressLine1, CustomerConstraints.BillingAddressLine1MaxLength, nameof(billingAddressLine1));
        this.BillingAddressLine2 = NormalizeOptional(billingAddressLine2, CustomerConstraints.BillingAddressLine2MaxLength, nameof(billingAddressLine2));
        this.CityProvinceState = NormalizeOptional(cityProvinceState, CustomerConstraints.CityProvinceStateMaxLength, nameof(cityProvinceState));
        this.PostalCode = NormalizeOptional(postalCode, CustomerConstraints.PostalCodeMaxLength, nameof(postalCode));
        this.Country = NormalizeOptional(country, CustomerConstraints.CountryMaxLength, nameof(country));
        this.ContactName = NormalizeOptional(contactName, CustomerConstraints.ContactNameMaxLength, nameof(contactName));
        this.Notes = NormalizeOptional(notes, CustomerConstraints.NotesMaxLength, nameof(notes));
    }

    private static string NormalizeRequired(string value, int maximumLength, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return NormalizeLength(value.Trim(), maximumLength, parameterName);
    }

    private static string? NormalizeOptional(string? value, int maximumLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return NormalizeLength(value.Trim(), maximumLength, parameterName);
    }

    private static string NormalizeLength(string value, int maximumLength, string parameterName)
    {
        if (value.Length > maximumLength)
        {
            throw new ArgumentException($"Value cannot exceed {maximumLength} characters.", parameterName);
        }

        return value;
    }
}

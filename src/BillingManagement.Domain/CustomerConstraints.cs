namespace BillingManagement.Domain;

public static class CustomerConstraints
{
    public const int CustomerNameMaxLength = 200;
    public const int TaxIdMaxLength = 100;
    public const int EmailMaxLength = 254;
    public const int PhoneMaxLength = 100;
    public const int BillingAddressLine1MaxLength = 300;
    public const int BillingAddressLine2MaxLength = 300;
    public const int CityProvinceStateMaxLength = 150;
    public const int PostalCodeMaxLength = 50;
    public const int CountryMaxLength = 100;
    public const int ContactNameMaxLength = 200;
    public const int NotesMaxLength = 2000;
}

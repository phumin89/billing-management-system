namespace BillingManagement.Application.Abstractions.CompanyMedia;

public sealed record CompanyMediaStorageKey
{
    public string Value { get; }

    private CompanyMediaStorageKey(string value)
    {
        this.Value = value;
    }

    public static CompanyMediaStorageKey Create() => new(Guid.NewGuid().ToString("N"));

    public static CompanyMediaStorageKey Parse(string value)
    {
        if (!Guid.TryParseExact(value, "N", out var identifier))
        {
            throw new ArgumentException("Company media storage keys must be opaque GUID values.", nameof(value));
        }

        return new CompanyMediaStorageKey(identifier.ToString("N"));
    }
}

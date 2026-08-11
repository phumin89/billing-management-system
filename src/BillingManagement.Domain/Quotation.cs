namespace BillingManagement.Domain;

public sealed class Quotation
{
    private readonly List<QuotationItem> items = [];

    private Quotation()
    {
    }

    public Guid Id { get; private set; }
    public string Number { get; private set; } = string.Empty;
    public string SellerCompanyName { get; private set; } = string.Empty;
    public string SellerAddress { get; private set; } = string.Empty;
    public string? SellerTaxId { get; private set; }
    public string? SellerPhone { get; private set; }
    public string? SellerEmail { get; private set; }
    public string? SellerWebsite { get; private set; }
    public string? SellerRegistrationNumber { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string? CustomerAddress { get; private set; }
    public string? CustomerTaxId { get; private set; }
    public DateOnly IssueDate { get; private set; }
    public DateOnly ValidUntil { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public IReadOnlyList<QuotationItem> Items => this.items;
    public decimal Subtotal => this.items.Sum(item => item.LineSubtotal);
    public decimal TaxTotal => this.items.Sum(item => item.TaxAmount);
    public decimal Total => this.items.Sum(item => item.LineTotal);

    public static Quotation Create(
        Guid id,
        string number,
        SellerSnapshot seller,
        Guid customerId,
        string customerName,
        string? customerAddress,
        string? customerTaxId,
        DateOnly issueDate,
        DateOnly validUntil,
        string currency,
        IReadOnlyCollection<QuotationItemInput> items)
    {
        if (id == Guid.Empty || customerId == Guid.Empty)
        {
            throw new ArgumentException("Document and customer IDs are required.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        ArgumentException.ThrowIfNullOrWhiteSpace(seller.CompanyName);
        ArgumentException.ThrowIfNullOrWhiteSpace(seller.Address);
        ArgumentException.ThrowIfNullOrWhiteSpace(customerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        if (validUntil < issueDate)
        {
            throw new ArgumentException("Valid-until date cannot precede the issue date.");
        }

        if (items.Count == 0)
        {
            throw new ArgumentException("At least one quotation item is required.", nameof(items));
        }

        var quotation = new Quotation
        {
            Id = id,
            Number = number.Trim(),
            SellerCompanyName = seller.CompanyName.Trim(),
            SellerAddress = seller.Address.Trim(),
            SellerTaxId = NormalizeOptional(seller.TaxId),
            SellerPhone = NormalizeOptional(seller.Phone),
            SellerEmail = NormalizeOptional(seller.Email),
            SellerWebsite = NormalizeOptional(seller.Website),
            SellerRegistrationNumber = NormalizeOptional(seller.RegistrationNumber),
            CustomerId = customerId,
            CustomerName = customerName.Trim(),
            CustomerAddress = NormalizeOptional(customerAddress),
            CustomerTaxId = NormalizeOptional(customerTaxId),
            IssueDate = issueDate,
            ValidUntil = validUntil,
            Currency = currency.Trim().ToUpperInvariant()
        };
        quotation.items.AddRange(items.Select((item, position) =>
            new QuotationItem(Guid.NewGuid(), position, item.Description, item.Quantity, item.UnitPrice, item.TaxRate)));
        return quotation;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

namespace BillingManagement.Domain;

public sealed class Quotation
{
    private readonly List<QuotationItem> items = [];

    private Quotation()
    {
    }

    public Guid Id { get; private set; }
    public string Number { get; private set; } = string.Empty;
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
            CustomerId = customerId,
            CustomerName = customerName.Trim(),
            CustomerAddress = NormalizeOptional(customerAddress),
            CustomerTaxId = NormalizeOptional(customerTaxId),
            IssueDate = issueDate,
            ValidUntil = validUntil,
            Currency = currency.Trim().ToUpperInvariant()
        };
        quotation.items.AddRange(items.Select(item =>
            new QuotationItem(Guid.NewGuid(), item.Description, item.Quantity, item.UnitPrice, item.TaxRate)));
        return quotation;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

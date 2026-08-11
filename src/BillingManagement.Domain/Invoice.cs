namespace BillingManagement.Domain;

public sealed class Invoice
{
    private readonly List<InvoiceItem> items = [];

    private Invoice()
    {
    }

    public Guid Id { get; private set; }
    public string Number { get; private set; } = string.Empty;
    public Guid QuotationId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string? CustomerAddress { get; private set; }
    public string? CustomerTaxId { get; private set; }
    public DateOnly IssueDate { get; private set; }
    public DateOnly DueDate { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public IReadOnlyList<InvoiceItem> Items => this.items;
    public decimal Subtotal => this.items.Sum(item => item.LineSubtotal);
    public decimal TaxTotal => this.items.Sum(item => item.TaxAmount);
    public decimal Total => this.items.Sum(item => item.LineTotal);

    public static Invoice CreateFromQuotation(
        Guid id,
        string number,
        Quotation quotation,
        DateOnly issueDate,
        DateOnly dueDate)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Invoice ID is required.", nameof(id));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        if (dueDate < issueDate)
        {
            throw new ArgumentException("Due date cannot precede the issue date.");
        }

        var invoice = new Invoice
        {
            Id = id,
            Number = number.Trim(),
            QuotationId = quotation.Id,
            CustomerId = quotation.CustomerId,
            CustomerName = quotation.CustomerName,
            CustomerAddress = quotation.CustomerAddress,
            CustomerTaxId = quotation.CustomerTaxId,
            IssueDate = issueDate,
            DueDate = dueDate,
            Currency = quotation.Currency
        };
        invoice.items.AddRange(quotation.Items.Select(item => new InvoiceItem(
            Guid.NewGuid(), item.Description, item.Quantity, item.UnitPrice, item.TaxRate)));
        return invoice;
    }
}

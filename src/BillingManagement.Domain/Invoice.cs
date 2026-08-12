namespace BillingManagement.Domain;

public sealed class Invoice
{
    private readonly List<InvoiceItem> items = [];

    private Invoice()
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
    public Guid QuotationId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string? CustomerAddress { get; private set; }
    public string? CustomerTaxId { get; private set; }
    public DateOnly IssueDate { get; private set; }
    public DateOnly DueDate { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public InvoiceStatus Status { get; private set; }
    public DateOnly? PaidDate { get; private set; }
    public decimal? AmountPaid { get; private set; }
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
            SellerCompanyName = quotation.SellerCompanyName,
            SellerAddress = quotation.SellerAddress,
            SellerTaxId = quotation.SellerTaxId,
            SellerPhone = quotation.SellerPhone,
            SellerEmail = quotation.SellerEmail,
            SellerWebsite = quotation.SellerWebsite,
            SellerRegistrationNumber = quotation.SellerRegistrationNumber,
            QuotationId = quotation.Id,
            CustomerId = quotation.CustomerId,
            CustomerName = quotation.CustomerName,
            CustomerAddress = quotation.CustomerAddress,
            CustomerTaxId = quotation.CustomerTaxId,
            IssueDate = issueDate,
            DueDate = dueDate,
            Currency = quotation.Currency,
            Status = InvoiceStatus.Issued
        };
        invoice.items.AddRange(quotation.Items.OrderBy(item => item.Position).Select((item, position) => new InvoiceItem(
            Guid.NewGuid(), position, item.Description, item.Quantity, item.UnitPrice, item.TaxRate)));
        return invoice;
    }

    public void MarkPaid(DateOnly paidDate, decimal amountPaid)
    {
        if (this.Status != InvoiceStatus.Issued)
        {
            throw new InvalidOperationException("Only an issued invoice can be marked as paid.");
        }

        if (amountPaid != this.Total)
        {
            throw new ArgumentException("Amount paid must equal the invoice total.", nameof(amountPaid));
        }

        this.Status = InvoiceStatus.Paid;
        this.PaidDate = paidDate;
        this.AmountPaid = amountPaid;
    }

    public void Cancel()
    {
        if (this.Status != InvoiceStatus.Issued)
        {
            throw new InvalidOperationException("Only an issued invoice can be cancelled.");
        }

        this.Status = InvoiceStatus.Cancelled;
    }
}

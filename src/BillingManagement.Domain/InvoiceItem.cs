namespace BillingManagement.Domain;

public sealed class InvoiceItem
{
    private InvoiceItem()
    {
    }

    internal InvoiceItem(Guid id, int position, string description, decimal quantity, decimal unitPrice, decimal taxRate)
    {
        this.Id = id;
        this.Position = position;
        this.Description = description;
        this.Quantity = quantity;
        this.UnitPrice = unitPrice;
        this.TaxRate = taxRate;
    }

    public Guid Id { get; private set; }
    public int Position { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal LineSubtotal => decimal.Round(this.Quantity * this.UnitPrice, 2);
    public decimal TaxAmount => decimal.Round(this.LineSubtotal * this.TaxRate / 100, 2);
    public decimal LineTotal => this.LineSubtotal + this.TaxAmount;
}

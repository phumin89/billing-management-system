namespace BillingManagement.Domain;

public sealed class QuotationItem
{
    private QuotationItem()
    {
    }

    internal QuotationItem(Guid id, int position, string description, decimal quantity, decimal unitPrice, decimal taxRate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        if (unitPrice < 0 || taxRate < 0 || taxRate > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Prices and tax rates must be valid.");
        }

        this.Id = id;
        this.Position = position;
        this.Description = description.Trim();
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

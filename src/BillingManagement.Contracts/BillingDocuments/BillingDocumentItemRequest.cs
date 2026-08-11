using System.ComponentModel.DataAnnotations;

namespace BillingManagement.Contracts.BillingDocuments;

public sealed class BillingDocumentItemRequest
{
    [Required, MaxLength(500)] public string? Description { get; set; }
    [Range(typeof(decimal), "0.0001", "99999999999999")] public decimal Quantity { get; set; }
    [Range(typeof(decimal), "0", "99999999999999")] public decimal UnitPrice { get; set; }
    [Range(typeof(decimal), "0", "100")] public decimal TaxRate { get; set; }
}

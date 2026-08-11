using System.ComponentModel.DataAnnotations;

namespace BillingManagement.Contracts.BillingDocuments;

public sealed class CreateInvoiceRequest
{
    [Required, MaxLength(50)] public string? Number { get; set; }
    public Guid QuotationId { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly DueDate { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace BillingManagement.Contracts.BillingDocuments;

public sealed class CreateQuotationRequest
{
    [Required, MaxLength(50)] public string? Number { get; set; }
    public Guid CustomerId { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly ValidUntil { get; set; }
    [Required, StringLength(3, MinimumLength = 3)] public string? Currency { get; set; }
    [Required, MinLength(1)] public List<BillingDocumentItemRequest> Items { get; set; } = [];
}

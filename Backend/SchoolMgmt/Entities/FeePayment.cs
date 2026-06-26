using SchoolMgmt.Enums;

namespace SchoolMgmt.Entities;

public class FeePayment
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;
    public Guid FeeStructureId { get; set; }
    public FeeStructure FeeStructure { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string ReceiptNumber { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public Guid RecordedByUserId { get; set; }
    public ApplicationUser RecordedBy { get; set; } = null!;
    public string? ReceiptUrl { get; set; }           // Azure Blob PDF URL
    public decimal DiscountAmount { get; set; } = 0;
}
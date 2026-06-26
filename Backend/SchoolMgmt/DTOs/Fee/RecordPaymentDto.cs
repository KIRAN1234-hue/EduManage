namespace SchoolMgmt.DTOs.Fee;

public class RecordPaymentDto
{
    public Guid StudentId { get; set; }
    public Guid FeeStructureId { get; set; }
    public decimal Amount { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string? ReceiptUrl { get; set; }
}
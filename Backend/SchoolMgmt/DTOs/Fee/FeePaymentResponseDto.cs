namespace SchoolMgmt.DTOs.Fee;

public class FeePaymentResponseDto
{
    public Guid Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public string TermName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal NetAmount => Amount - DiscountAmount;
    public DateTime PaymentDate { get; set; }   // PaymentDate — matches entity
    public string ReceiptNumber { get; set; } = string.Empty;  // matches entity
    public string? ReceiptUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string RecordedBy { get; set; } = string.Empty;
    public decimal TotalFee { get; set; }
    public decimal Balance { get; set; }
}
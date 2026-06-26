namespace SchoolMgmt.DTOs.Fee;

public class ParentPaymentDto
{
    public Guid FeeStructureId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Online";
    public string? Remarks { get; set; }
}
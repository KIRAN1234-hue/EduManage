namespace SchoolMgmt.DTOs.Fee;

public class StudentFeeStatusDto
{
    public string StudentName { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public IEnumerable<FeeTermStatusDto> Terms { get; set; } = new List<FeeTermStatusDto>();
    public decimal TotalDue { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal Balance { get; set; }
}

public class FeeTermStatusDto
{
    public Guid FeeStructureId { get; set; }   // ADD — needed for payment
    public string TermName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Paid { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateOnly DueDate { get; set; }
    public string? ReceiptNumber { get; set; }
}
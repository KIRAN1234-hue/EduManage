namespace SchoolMgmt.DTOs.Fee;

public class FeeStructureResponseDto
{
    public Guid Id { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public string TermName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly DueDate { get; set; }
    public bool DiscountAllowed { get; set; }
    public string Description { get; set; } = string.Empty;
    public int PaidCount { get; set; }
    public decimal TotalCollected { get; set; }
    public bool IsOverdue => DueDate < DateOnly.FromDateTime(DateTime.Today);
}
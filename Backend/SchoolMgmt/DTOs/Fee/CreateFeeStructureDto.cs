namespace SchoolMgmt.DTOs.Fee;

public class CreateFeeStructureDto
{
    public Guid ClassId { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
    public string TermName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly DueDate { get; set; }
    public bool DiscountAllowed { get; set; } = false;
    public string Description { get; set; } = string.Empty;  // entity HAS this
}
using SchoolMgmt.Enums;
namespace SchoolMgmt.DTOs.Complaint;

public class ComplaintResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string SubmittedBy { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public string? ResolutionRemark { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class UpdateComplaintDto
{
    public Guid? AssignedToUserId { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public string? ResolutionRemark { get; set; }
}
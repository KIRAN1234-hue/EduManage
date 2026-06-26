using SchoolMgmt.Enums;

namespace SchoolMgmt.Entities;

public class Complaint
{
    public Guid Id { get; set; }

    public Guid SubmittedByUserId { get; set; }
    public ApplicationUser SubmittedBy { get; set; } = null!;

    public Guid? AssignedToUserId { get; set; }
    public ApplicationUser? AssignedTo { get; set; }

    public ComplaintCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public ComplaintStatus Status { get; set; } = ComplaintStatus.Open;
    public string? ResolutionRemark { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}
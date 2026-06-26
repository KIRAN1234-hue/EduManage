using SchoolMgmt.Enums;

namespace SchoolMgmt.Entities;

public class LeaveApplication
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public string? ApprovalRemark { get; set; }

    public Guid? ApprovedByTeacherId { get; set; }
    public Teacher? ApprovedByTeacher { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime AppliedOn { get; set; } = DateTime.UtcNow;
    public Guid? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public ApplicationUser? ApprovedBy { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public string LeaveType { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}
using SchoolMgmt.Enums;

namespace SchoolMgmt.Entities;

public class Submission
{
    public Guid Id { get; set; }

    public Guid AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public string FilePath { get; set; } = string.Empty;   // Azure Blob URL
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // Calculated at service layer — SubmittedAt.Date > Assignment.DueDate.Date
    public bool IsLate { get; set; } = false;

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;
    public string? TeacherRemark { get; set; }

    public string Content { get; set; } = string.Empty;
    public decimal? MarksAwarded { get; set; }
    public string? Feedback { get; set; }
}
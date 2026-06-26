namespace SchoolMgmt.DTOs.Assignment;

public class SubmissionResponseDto
{
    public Guid Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public DateTime SubmittedAt { get; set; }
    public bool IsLate { get; set; }
    public decimal? MarksAwarded { get; set; }
    public string? TeacherRemark { get; set; }
    public string? Feedback { get; set; }
    public string Status { get; set; } = string.Empty;
    public string AssignmentTitle { get; set; } = string.Empty;
}
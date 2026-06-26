namespace SchoolMgmt.DTOs.Assignment;

public class AssignmentResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int TotalMarks { get; set; }
    public string? FilePath { get; set; }
    public bool IsActive { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int SubmissionCount { get; set; }
    public bool IsOverdue => DateTime.UtcNow > DueDate;
}
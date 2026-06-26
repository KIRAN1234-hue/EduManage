namespace SchoolMgmt.DTOs.Marks;

public class MarkResponseDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string ExamType { get; set; } = string.Empty;
    public decimal MarksObtained { get; set; }
    public int MaxMarks { get; set; }
    public decimal Percentage { get; set; }
    public string Grade { get; set; } = string.Empty;
    public DateTime EnteredAt { get; set; }
    public string EnteredByTeacher { get; set; } = string.Empty;
}
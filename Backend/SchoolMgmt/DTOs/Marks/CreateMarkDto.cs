namespace SchoolMgmt.DTOs.Marks;

// One record per student — sent as a list for batch entry
public class CreateMarkDto
{
    public Guid StudentId { get; set; }
    public Guid SubjectId { get; set; }
    public string ExamType { get; set; } = string.Empty; // UnitTest, MidTerm, Final, Practical
    public decimal MarksObtained { get; set; }
    public int MaxMarks { get; set; }
}
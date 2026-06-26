using SchoolMgmt.Enums;

namespace SchoolMgmt.Entities;

public class Mark
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public Guid EnteredByTeacherId { get; set; }
    public Teacher EnteredByTeacher { get; set; } = null!;

    public ExamType ExamType { get; set; }
    public decimal MarksObtained { get; set; }
    public int MaxMarks { get; set; }

    // Set by GradeService — not manually entered
    public string Grade { get; set; } = string.Empty;

    public DateTime EnteredAt { get; set; } = DateTime.UtcNow;
}
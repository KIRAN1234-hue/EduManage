namespace SchoolMgmt.DTOs.Marks;

public class ReportCardDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public decimal OverallPercentage { get; set; }
    public string OverallGrade { get; set; } = string.Empty;
    public List<SubjectReportDto> Subjects { get; set; } = new();
}

public class SubjectReportDto
{
    public string SubjectName { get; set; } = string.Empty;
    public string SubjectCode { get; set; } = string.Empty;
    public List<ExamMarkDto> Exams { get; set; } = new();
    public decimal SubjectPercentage { get; set; }
    public string SubjectGrade { get; set; } = string.Empty;
}

public class ExamMarkDto
{
    public string ExamType { get; set; } = string.Empty;
    public decimal MarksObtained { get; set; }
    public int MaxMarks { get; set; }
    public string Grade { get; set; } = string.Empty;
}
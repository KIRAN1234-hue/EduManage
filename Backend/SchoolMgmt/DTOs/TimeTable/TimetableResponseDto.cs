using SchoolMgmt.Enums;

namespace SchoolMgmt.DTOs.Timetable;

public class TimetableResponseDto
{
    public Guid Id { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string DayOfWeek { get; set; } = string.Empty;
    public int PeriodNumber { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
}
using SchoolMgmt.Enums;

namespace SchoolMgmt.DTOs.Timetable;

public class CreateTimetableDto
{
    public Guid ClassId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid TeacherId { get; set; }
    public SchoolDay DayOfWeek { get; set; }
    public int PeriodNumber { get; set; }
    public string StartTime { get; set; } = string.Empty;  // "08:00"
    public string EndTime { get; set; } = string.Empty;  // "08:45"
    public string AcademicYear { get; set; } = string.Empty;
}
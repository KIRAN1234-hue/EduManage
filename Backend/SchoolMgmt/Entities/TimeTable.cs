using SchoolMgmt.Enums;

namespace SchoolMgmt.Entities;

public class Timetable
{
    public Guid Id { get; set; }

    public Guid ClassId { get; set; }
    public Class Class { get; set; } = null!;

    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public Guid TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;

    public SchoolDay DayOfWeek { get; set; }
    public int PeriodNumber { get; set; }   // 1 to 8
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
}
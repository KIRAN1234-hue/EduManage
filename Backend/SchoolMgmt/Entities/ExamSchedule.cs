using SchoolMgmt.Enums;

namespace SchoolMgmt.Entities;

public class ExamSchedule
{
    public Guid Id { get; set; }

    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public Guid ClassId { get; set; }
    public Class Class { get; set; } = null!;

    public Guid InvigilatorTeacherId { get; set; }
    public Teacher InvigilatorTeacher { get; set; } = null!;

    public ExamType ExamType { get; set; }
    public DateOnly ExamDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string RoomNumber { get; set; } = string.Empty;

    // Flipped to true after exam date passes — unlocks marks entry
    public bool MarksEntryOpen { get; set; } = false;
}
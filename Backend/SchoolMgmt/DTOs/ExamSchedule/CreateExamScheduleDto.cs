using SchoolMgmt.Enums;

namespace SchoolMgmt.DTOs.ExamSchedule;

public class CreateExamScheduleDto
{
    public Guid SubjectId { get; set; }
    public Guid ClassId { get; set; }
    public Guid InvigilatorTeacherId { get; set; }
    public ExamType ExamType { get; set; }
    public DateOnly ExamDate { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
}
using SchoolMgmt.Enums;

namespace SchoolMgmt.DTOs.ExamSchedule;

public class ExamScheduleResponseDto
{
    public Guid Id { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string InvigilatorTeacher { get; set; } = string.Empty;
    public string ExamType { get; set; } = string.Empty;
    public DateOnly ExamDate { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public bool MarksEntryOpen { get; set; }
}
using SchoolMgmt.Enums;

namespace SchoolMgmt.Entities;

public class Attendance
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public Guid? MarkedByTeacherId { get; set; }   // was Guid
    public Teacher? MarkedByTeacher { get; set; }  // was Teacher = null!

    public DateOnly Date { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Remarks { get; set; }
}
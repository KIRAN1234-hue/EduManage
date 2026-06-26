using System.Security.Claims;
using System.Xml;

namespace SchoolMgmt.Entities;

public class Teacher
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string Qualification { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public DateOnly JoiningDate { get; set; }
    public bool IsClassTeacher { get; set; } = false;

    public Guid? ClassId { get; set; }
    public Class? Class { get; set; }

    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    public ICollection<Attendance> MarkedAttendances { get; set; } = new List<Attendance>();
    public ICollection<Mark> EnteredMarks { get; set; } = new List<Mark>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<LeaveApplication> ApprovedLeaves { get; set; } = new List<LeaveApplication>();
    public ICollection<ExamSchedule> InvigilatedExams { get; set; } = new List<ExamSchedule>();
    public ICollection<Timetable> TimetableSlots { get; set; } = new List<Timetable>();
}
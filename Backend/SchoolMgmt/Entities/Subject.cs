using SchoolMgmt.Enums;
using System.Xml;

namespace SchoolMgmt.Entities;

public class Subject
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int MaxMarks { get; set; } = 100;
    public bool IsElective { get; set; } = false;
    public SubjectType SubjectType { get; set; } = SubjectType.Theory;

    public Guid ClassId { get; set; }
    public Class Class { get; set; } = null!;

    public Guid? TeacherId { get; set; }
    public Teacher? Teacher { get; set; } = null!;

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<Mark> Marks { get; set; } = new List<Mark>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<ExamSchedule> ExamSchedules { get; set; } = new List<ExamSchedule>();
    public ICollection<Timetable> Timetables { get; set; } = new List<Timetable>();
}
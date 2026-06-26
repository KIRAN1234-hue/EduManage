using System.Xml;

namespace SchoolMgmt.Entities;

public class Class
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public int MaxStrength { get; set; } = 40;

    public Guid? ClassTeacherId { get; set; }
    public Teacher? ClassTeacher { get; set; }

    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<FeeStructure> FeeStructures { get; set; } = new List<FeeStructure>();
    public ICollection<ExamSchedule> ExamSchedules { get; set; } = new List<ExamSchedule>();
    public ICollection<Timetable> Timetables { get; set; } = new List<Timetable>();
}
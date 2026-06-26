using System.Security.Claims;

namespace SchoolMgmt.Entities;

public class Student
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public Guid ClassId { get; set; }
    public Class Class { get; set; } = null!;

    public Guid? ParentId { get; set; }
    public Parent? Parent { get; set; }

    public string RollNumber { get; set; } = string.Empty;
    public DateOnly AdmissionDate { get; set; }
    public DateOnly DateOfBirth { get; set; }

    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<Mark> Marks { get; set; } = new List<Mark>();
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    public ICollection<FeePayment> FeePayments { get; set; } = new List<FeePayment>();
    public ICollection<LeaveApplication> LeaveApplications { get; set; } = new List<LeaveApplication>();
    public ICollection<BookIssue> BookIssues { get; set; } = new List<BookIssue>();
}
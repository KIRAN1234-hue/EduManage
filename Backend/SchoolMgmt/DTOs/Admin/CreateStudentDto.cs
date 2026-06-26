namespace SchoolMgmt.DTOs.Admin;

public class CreateStudentDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public DateOnly AdmissionDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
}
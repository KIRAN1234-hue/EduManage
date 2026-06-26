namespace SchoolMgmt.DTOs.Admin;

public class CreateTeacherDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public DateOnly JoiningDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public Guid? ClassId { get; set; }
    public bool IsClassTeacher { get; set; } = false;
}
namespace SchoolMgmt.DTOs.Admin;

public class TeacherResponseDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public string? ClassName { get; set; }
    public bool IsActive { get; set; }

    // Returned only on creation — share this with teacher for registration
    public string? InviteToken { get; set; }
    public Guid UserId { get; set; }
    public string? SubjectName { get; set; }
}
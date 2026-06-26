namespace SchoolMgmt.DTOs.Admin;

public class StudentResponseDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public DateOnly AdmissionDate { get; set; }
    public bool IsActive { get; set; }

    // Returned only on creation — share this with student
    public string? InviteToken { get; set; }
}
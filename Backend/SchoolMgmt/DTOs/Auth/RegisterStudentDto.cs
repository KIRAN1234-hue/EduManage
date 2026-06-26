namespace SchoolMgmt.DTOs.Auth;

public class RegisterStudentDto
{
    public string RollNumber { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string Email { get; set; } = string.Empty;  // ADD THIS
    public string NewPassword { get; set; } = string.Empty;
}
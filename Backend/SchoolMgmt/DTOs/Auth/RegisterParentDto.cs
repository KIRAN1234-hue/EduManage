namespace SchoolMgmt.DTOs.Auth;

public class RegisterParentDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string StudentRollNumber { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;  // ADD THIS
    public string? PhoneNumber { get; set; }
}
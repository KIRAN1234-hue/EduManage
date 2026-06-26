namespace SchoolMgmt.DTOs.Auth;

public class RegisterTeacherDto
{
    public string Email { get; set; } = string.Empty;
    public string InviteToken { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
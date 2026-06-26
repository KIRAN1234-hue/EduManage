using SchoolMgmt.DTOs.Auth;

namespace SchoolMgmt.Services.Interfaces;

public interface IAuthService
{
    // Validates credentials, returns JWT + refresh token
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

    // Validates refresh token, returns new JWT + new refresh token
    Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);

    // Revokes refresh token — user is logged out
    Task LogoutAsync(string refreshToken);

    Task RegisterTeacherAsync(RegisterTeacherDto dto);
    Task RegisterStudentAsync(RegisterStudentDto dto);
    Task RegisterParentAsync(RegisterParentDto dto);
    Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
}
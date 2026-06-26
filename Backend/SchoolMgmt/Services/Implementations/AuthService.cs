using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SchoolMgmt.DTOs.Auth;
using SchoolMgmt.DTOs.Auth;
using SchoolMgmt.Entities;
using SchoolMgmt.Enums;
using SchoolMgmt.Repositories.Implementations;
using SchoolMgmt.Repositories.Interfaces;
using SchoolMgmt.Services.Interfaces;
using SchoolMgmt.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SchoolMgmt.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings.Value;
    }

    // ── Login ─────────────────────────────────────────────────────────────────
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        // Step 1 — Find user by email
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("Invalid email or password.");

        // Step 2 — Check if account is locked out
        if (await _userManager.IsLockedOutAsync(user))
            throw new UnauthorizedAccessException(
                "Account locked due to multiple failed attempts. Try again in 15 minutes.");

        // Step 3 — Validate password
        var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!isPasswordCorrect)
        {
            // Increment failed access count — triggers lockout after 5 attempts
            await _userManager.AccessFailedAsync(user);
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Step 4 — Reset failed access count on successful login
        await _userManager.ResetAccessFailedCountAsync(user);

        // Step 5 — Get Identity role (Principal / Teacher / Student / Parent)
        var roles = await _userManager.GetRolesAsync(user);

        // Step 6 — Generate both tokens
        var accessToken = GenerateAccessToken(user, roles);
        var refreshTokenValue = GenerateRefreshToken();

        // Step 7 — Persist refresh token to database
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            FullName = user.FullName,
            Email = user.Email!,
            Role = roles.FirstOrDefault() ?? string.Empty,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes)
        };
    }

    // ── Refresh Token ─────────────────────────────────────────────────────────
    public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
    {
        // Step 1 — Find token in DB — must exist, not revoked, not expired
        var storedToken = await _unitOfWork.RefreshTokens
            .FirstOrDefaultAsync(rt =>
                rt.Token == refreshToken &&
                !rt.IsRevoked &&
                rt.ExpiresAt > DateTime.UtcNow);

        if (storedToken is null)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        // Step 2 — Load the user this token belongs to
        var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());

        if (user is null || !user.IsActive)
            throw new UnauthorizedAccessException("User not found or account is inactive.");

        // Step 3 — Revoke the old refresh token (rotation — each token used only once)
        storedToken.IsRevoked = true;
        _unitOfWork.RefreshTokens.Update(storedToken);

        // Step 4 — Generate new tokens
        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = GenerateAccessToken(user, roles);
        var newRefreshTokenValue = GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.RefreshTokens.AddAsync(newRefreshToken);
        await _unitOfWork.SaveChangesAsync();

        return new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenValue,
            FullName = user.FullName,
            Email = user.Email!,
            Role = roles.FirstOrDefault() ?? string.Empty,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes)
        };
    }

    // ── Logout ────────────────────────────────────────────────────────────────
    public async Task LogoutAsync(string refreshToken)
    {
        var storedToken = await _unitOfWork.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

        // If token not found or already revoked — nothing to do, logout is still successful
        if (storedToken is null) return;

        storedToken.IsRevoked = true;
        _unitOfWork.RefreshTokens.Update(storedToken);
        await _unitOfWork.SaveChangesAsync();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private string GenerateAccessToken(ApplicationUser user, IList<string> roles)
    {
        // Claims are pieces of information embedded inside the JWT token
        // Angular reads these to know who the user is and what role they have
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email,          user.Email!),
            new Claim(ClaimTypes.Name,           user.FullName),
            new Claim(ClaimTypes.Role,           roles.FirstOrDefault() ?? string.Empty),
            new Claim("roleType",                user.RoleType.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        // Cryptographically secure random 64-byte string
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

// ── Register Teacher ──────────────────────────────────────────────────────
public async Task RegisterTeacherAsync(RegisterTeacherDto dto)
{
    var user = await _userManager.FindByEmailAsync(dto.Email)
        ?? throw new KeyNotFoundException("No account found with this email.");

    if (user.EmailConfirmed)
        throw new InvalidOperationException("This account has already been registered.");

    // Validate invite token and reset password
    var result = await _userManager.ResetPasswordAsync(user, dto.InviteToken, dto.NewPassword);

    if (!result.Succeeded)
    {
        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        throw new InvalidOperationException($"Invalid or expired invite token. {errors}");
    }

    // Confirm email — registration complete
    user.EmailConfirmed = true;
    await _userManager.UpdateAsync(user);
}

    // ── Register Student ──────────────────────────────────────────────────────
    public async Task RegisterStudentAsync(RegisterStudentDto dto)
    {
        // Step 1 — Find student by rollNumber + dateOfBirth
        var matchingStudents = await _unitOfWork.Students.FindAsync(s =>
            s.RollNumber == dto.RollNumber &&
            s.DateOfBirth == dto.DateOfBirth);

        if (!matchingStudents.Any())
            throw new KeyNotFoundException(
                "No student found with this roll number and date of birth.");

        // Step 2 — Among matches, find the one with matching email
        // This handles same rollNumber + DOB in different classes
        Student? student = null;
        foreach (var s in matchingStudents)
        {
            var user = await _userManager.FindByIdAsync(s.UserId.ToString());
            if (user != null &&
                user.Email!.ToLower() == dto.Email.ToLower())
            {
                student = s;
                break;
            }
        }

        if (student == null)
            throw new KeyNotFoundException(
                "No student found matching this roll number, date of birth, and email.");

        // Step 3 — Get the ApplicationUser
        var studentUser = await _userManager.FindByIdAsync(student.UserId.ToString())
            ?? throw new KeyNotFoundException("User account not found.");

        if (studentUser.EmailConfirmed)
            throw new InvalidOperationException(
                "This account has already been registered. Please login directly.");

        // Step 4 — Set password using reset token
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(studentUser);
        var result = await _userManager.ResetPasswordAsync(
            studentUser, resetToken, dto.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to set password: {errors}");
        }

        // Step 5 — Confirm email
        studentUser.EmailConfirmed = true;
        await _userManager.UpdateAsync(studentUser);
    }
    // ── Register Parent ───────────────────────────────────────────────────────
    public async Task RegisterParentAsync(RegisterParentDto dto)
    {
        // Step 1 — Find all students with this roll number
        var matchingStudents = await _unitOfWork.Students.FindAsync(s =>
            s.RollNumber == dto.StudentRollNumber);

        if (!matchingStudents.Any())
            throw new KeyNotFoundException(
                $"No student found with roll number {dto.StudentRollNumber}.");

        // Step 2 — Among matches, find the one whose email matches
        // Handles same roll number across different classes
        Student? student = null;
        foreach (var s in matchingStudents)
        {
            var stuUser = await _userManager.FindByIdAsync(s.UserId.ToString());
            if (stuUser != null &&
                stuUser.Email!.ToLower() == dto.StudentEmail.ToLower())
            {
                student = s;
                break;
            }
        }

        if (student == null)
            throw new KeyNotFoundException(
                "No student found matching this roll number and student email. " +
                "Please contact the school for the correct student email.");

        // Step 3 — Check student already has a parent
        if (student.ParentId.HasValue)
            throw new InvalidOperationException(
                "This student already has a parent account linked.");

        // Step 4 — Check parent email not already taken
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new InvalidOperationException(
                $"Email {dto.Email} is already registered.");

        // Step 5 — Create parent ApplicationUser
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName,
            Email = dto.Email,
            UserName = dto.Email,
            NormalizedEmail = dto.Email.ToUpper(),
            NormalizedUserName = dto.Email.ToUpper(),
            PhoneNumber = dto.PhoneNumber,
            EmailConfirmed = true,
            RoleType = RoleType.Parent,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create account: {errors}");
        }

        await _userManager.AddToRoleAsync(user, "Parent");

        // Step 6 — Create Parent entity
        var parent = new Parent
        {
            Id = Guid.NewGuid(),
            UserId = user.Id
        };

        await _unitOfWork.Parents.AddAsync(parent);

        // Step 7 — Link parent to student
        student.ParentId = parent.Id;
        _unitOfWork.Students.Update(student);

        await _unitOfWork.SaveChangesAsync();
    }
    // ── Change Password ───────────────────────────────────────────────────────
    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
{
    var user = await _userManager.FindByIdAsync(userId.ToString())
        ?? throw new KeyNotFoundException("User not found.");

    var result = await _userManager.ChangePasswordAsync(
        user, dto.CurrentPassword, dto.NewPassword);

    if (!result.Succeeded)
    {
        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        throw new InvalidOperationException(errors);
    }
}
}
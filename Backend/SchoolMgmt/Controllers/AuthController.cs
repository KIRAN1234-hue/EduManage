using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolMgmt.DTOs.Auth;
using SchoolMgmt.Entities;
using SchoolMgmt.Services.Implementations;
using SchoolMgmt.Services.Interfaces;
using System.Security.Claims;   

namespace SchoolMgmt.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(IAuthService authService, UserManager<ApplicationUser> userManager)
    {
        _authService = authService;
        _userManager = userManager;
    }

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    // POST api/auth/refresh
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    // POST api/auth/logout
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request)
    {
        await _authService.LogoutAsync(request.RefreshToken);
        return Ok(new { message = "Logged out successfully." });
    }

    // GET api/auth/me  — requires valid JWT token
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        // These values come directly from the JWT claims we embedded in GenerateAccessToken
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var fullName = User.FindFirstValue(ClaimTypes.Name);
        var role = User.FindFirstValue(ClaimTypes.Role);

        return Ok(new
        {
            UserId = userId,
            Email = email,
            FullName = fullName,
            Role = role
        });
    }

// POST /api/auth/register/teacher
[HttpPost("register/teacher")]
[AllowAnonymous]
public async Task<IActionResult> RegisterTeacher([FromBody] RegisterTeacherDto dto)
{
    try
    {
        await _authService.RegisterTeacherAsync(dto);
        return Ok(new { message = "Registration successful. You can now login." });
    }
    catch (KeyNotFoundException ex)
    {
        return NotFound(new { message = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}

// POST /api/auth/register/student
[HttpPost("register/student")]
[AllowAnonymous]
public async Task<IActionResult> RegisterStudent([FromBody] RegisterStudentDto dto)
{
    try
    {
        await _authService.RegisterStudentAsync(dto);
        return Ok(new { message = "Registration successful. You can now login." });
    }
    catch (KeyNotFoundException ex)
    {
        return NotFound(new { message = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}

// POST /api/auth/register/parent
[HttpPost("register/parent")]
[AllowAnonymous]
public async Task<IActionResult> RegisterParent([FromBody] RegisterParentDto dto)
{
    try
    {
        await _authService.RegisterParentAsync(dto);
        return Ok(new { message = "Parent account created. You can now login." });
    }
    catch (KeyNotFoundException ex)
    {
        return NotFound(new { message = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Conflict(new { message = ex.Message });
    }
}

// PUT /api/auth/change-password
[HttpPut("change-password")]
[Authorize]
public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
{
    var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!Guid.TryParse(userIdString, out var userId))
        return Unauthorized();

    try
    {
        await _authService.ChangePasswordAsync(userId, dto);
        return Ok(new { message = "Password changed successfully." });
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}

    // POST /api/auth/forgot-password
    // Returns reset token — in production this goes via email
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            // Don't reveal if email exists for security
            return Ok(new { message = "If this email exists, a reset token has been generated." });

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // In production: send via email
        // For development: return token directly
        return Ok(new
        {
            message = "Password reset token generated.",
            email = dto.Email,
            resetToken = token   //Going to Remove this in production — send via email only
        });
    }

    // POST /api/auth/reset-password
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return BadRequest(new { message = "Invalid request." });

        var result = await _userManager.ResetPasswordAsync(
            user, dto.ResetToken, dto.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return BadRequest(new { message = $"Reset failed: {errors}" });
        }

        return Ok(new { message = "Password reset successful. You can now login." });
    }
}
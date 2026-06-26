using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolMgmt.Repositories.Interfaces;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IUnitOfWork _unitOfWork;

    public DashboardController(
        IDashboardService dashboardService,
        IUnitOfWork unitOfWork)
    {
        _dashboardService = dashboardService;
        _unitOfWork = unitOfWork;
    }

    // GET /api/dashboard/principal
    [HttpGet("principal")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> GetPrincipalStats()
    {
        var stats = await _dashboardService.GetPrincipalStatsAsync();
        return Ok(stats);
    }

    // GET /api/dashboard/teacher
    [HttpGet("teacher")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetTeacherStats()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var teacher = await _unitOfWork.Teachers
            .FirstOrDefaultAsync(t => t.UserId == userId);

        if (teacher == null)
            return BadRequest(new { message = "Teacher profile not found." });

        var stats = await _dashboardService.GetTeacherStatsAsync(teacher.Id);
        return Ok(stats);
    }

    // GET /api/dashboard/student
    [HttpGet("student")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetStudentStats()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var student = await _unitOfWork.Students
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student == null)
            return BadRequest(new { message = "Student profile not found." });

        var stats = await _dashboardService.GetStudentStatsAsync(student.Id);
        return Ok(stats);
    }
}
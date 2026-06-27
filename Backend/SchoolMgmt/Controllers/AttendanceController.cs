using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolMgmt.DTOs.Attendance;
using SchoolMgmt.Repositories.Interfaces;
using SchoolMgmt.Services.Interfaces;
using System.Security.Claims;

namespace SchoolMgmt.Controllers;

[ApiController]
[Route("api/attendance")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;
    private readonly IUnitOfWork _unitOfWork;

    public AttendanceController(
        IAttendanceService attendanceService,
        IUnitOfWork unitOfWork)
    {
        _attendanceService = attendanceService;
        _unitOfWork = unitOfWork;
    }

    // ── POST /api/attendance
    [HttpPost]
    [Authorize(Roles = "Teacher,Principal")]
    public async Task<IActionResult> MarkBulkAttendance(
        [FromBody] List<MarkAttendanceDto> attendanceDtos)
    {
        if (attendanceDtos == null || !attendanceDtos.Any())
            return BadRequest(new { message = "Attendance list cannot be empty." });

        var role = User.FindFirstValue(ClaimTypes.Role);
        Guid? teacherId = null;

        // Only look up Teacher record if user is a Teacher
        // Principal can mark attendance without a Teacher record
        if (role?.ToLower() == "teacher")
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
                return Unauthorized();

            var teacher = await _unitOfWork.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
                return BadRequest(new { message = "Teacher profile not found." });

            teacherId = teacher.Id;
        }
        // For Principal — teacherId stays null, no Teacher record needed

        try
        {
            var result = await _attendanceService
                .MarkBulkAttendanceAsync(attendanceDtos, teacherId);
            return Ok(new { message = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── GET /api/attendance/student/{studentId}
    [HttpGet("student/{studentId}")]
    [Authorize(Roles = "Student,Teacher,Principal,Parent")]
    public async Task<IActionResult> GetStudentAttendance(Guid studentId)
    {
        try
        {
            var records = await _attendanceService
                .GetStudentAttendanceAsync(studentId);
            return Ok(records);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── GET /api/attendance/percentage/{studentId}
    [HttpGet("percentage/{studentId}")]
    [Authorize(Roles = "Student,Teacher,Principal,Parent")]
    public async Task<IActionResult> GetAttendancePercentage(Guid studentId)
    {
        try
        {
            var percentages = await _attendanceService
                .GetAttendancePercentageAsync(studentId);
            return Ok(percentages);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── GET /api/attendance/class/{classId}?date=2024-01-15 
    [HttpGet("class/{classId}")]
    [Authorize(Roles = "Teacher,Principal")]
    public async Task<IActionResult> GetClassAttendanceForDate(
        Guid classId, [FromQuery] DateOnly date)
    {
        var records = await _attendanceService
            .GetClassAttendanceForDateAsync(classId, date);
        return Ok(records);
    }

    // ── PUT /api/attendance/{attendanceId}
    [HttpPut("{attendanceId}")]
    [Authorize(Roles = "Teacher,Principal")]
    public async Task<IActionResult> UpdateAttendance(
        Guid attendanceId, [FromBody] UpdateAttendanceDto dto)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        Guid? teacherId = null;

        if (role?.ToLower() == "teacher")
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
                return Unauthorized();

            var teacher = await _unitOfWork.Teachers
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (teacher == null)
                return BadRequest(new { message = "Teacher profile not found." });

            teacherId = teacher.Id;
        }

        try
        {
            var updated = await _attendanceService
                .UpdateAttendanceAsync(attendanceId, dto, teacherId);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
    // GET /api/attendance/my — Student's own attendance records
    [HttpGet("my")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyAttendance()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var student = await _unitOfWork.Students
            .FirstOrDefaultAsync(s => s.UserId == userId);
        if (student == null)
            return BadRequest(new { message = "Student profile not found." });

        var records = await _attendanceService.GetStudentAttendanceAsync(student.Id);
        return Ok(records);
    }

    // GET /api/attendance/my-percentage — Student's own subject-wise %
    [HttpGet("my-percentage")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyAttendancePercentage()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var student = await _unitOfWork.Students
            .FirstOrDefaultAsync(s => s.UserId == userId);
        if (student == null)
            return BadRequest(new { message = "Student profile not found." });

        var percentages = await _attendanceService
            .GetAttendancePercentageAsync(student.Id);
        return Ok(percentages);
    }
}
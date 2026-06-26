using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolMgmt.DTOs.Marks;
using SchoolMgmt.Repositories.Interfaces;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Controllers;

[ApiController]
[Route("api/marks")]
[Authorize]
public class MarksController : ControllerBase
{
    private readonly IMarksService _marksService;
    private readonly IUnitOfWork _unitOfWork;

    public MarksController(IMarksService marksService, IUnitOfWork unitOfWork)
    {
        _marksService = marksService;
        _unitOfWork = unitOfWork;
    }

    // ── POST /api/marks ──────────────────────────────────────────────────────
    // Teacher submits marks for entire class in one request
    [HttpPost]
    [Authorize(Roles = "Teacher,Principal")]
    public async Task<IActionResult> CreateBulkMarks(
        [FromBody] List<CreateMarkDto> markDtos)
    {
        if (markDtos == null || !markDtos.Any())
            return BadRequest(new { message = "Marks list cannot be empty." });

        var teacherId = await GetTeacherIdAsync();
        if (teacherId is null)
            return BadRequest(new { message = "Teacher profile not found." });

        try
        {
            var result = await _marksService.CreateBulkMarksAsync(markDtos, teacherId.Value);
            return Ok(new { message = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── GET /api/marks/student/{studentId} ───────────────────────────────────
    [HttpGet("student/{studentId}")]
    [Authorize(Roles = "Student,Teacher,Principal,Parent")]
    public async Task<IActionResult> GetStudentMarks(Guid studentId)
    {
        var marks = await _marksService.GetStudentMarksAsync(studentId);
        return Ok(marks);
    }

    // ── GET /api/marks/report-card/{studentId} ───────────────────────────────
    [HttpGet("report-card/{studentId}")]
    [Authorize(Roles = "Student,Teacher,Principal,Parent")]
    public async Task<IActionResult> GetReportCard(Guid studentId)
    {
        try
        {
            var reportCard = await _marksService.GetReportCardAsync(studentId);
            return Ok(reportCard);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── GET /api/marks/class/{classId}?examType=UnitTest ────────────────────
    [HttpGet("class/{classId}")]
    [Authorize(Roles = "Teacher,Principal")]
    public async Task<IActionResult> GetClassMarks(
        Guid classId, [FromQuery] string examType)
    {
        if (string.IsNullOrEmpty(examType))
            return BadRequest(new { message = "examType query parameter is required." });

        var marks = await _marksService.GetClassMarksAsync(classId, examType);
        return Ok(marks);
    }

    // ── GET /api/marks/chart-data/{studentId} ────────────────────────────────
    // Returns Chart.js compatible JSON for Angular bar chart
    [HttpGet("chart-data/{studentId}")]
    [Authorize(Roles = "Student,Teacher,Principal,Parent")]
    public async Task<IActionResult> GetChartData(Guid studentId)
    {
        var chartData = await _marksService.GetChartDataAsync(studentId);
        return Ok(chartData);
    }

    // ── PUT /api/marks/{markId} ──────────────────────────────────────────────
    // Teacher corrects a marks entry
    [HttpPut("{markId}")]
    [Authorize(Roles = "Teacher,Principal")]
    public async Task<IActionResult> UpdateMark(
        Guid markId, [FromBody] UpdateMarkDto dto)
    {
        var teacherId = await GetTeacherIdAsync();
        if (teacherId is null)
            return BadRequest(new { message = "Teacher profile not found." });

        try
        {
            var updated = await _marksService.UpdateMarkAsync(markId, dto, teacherId.Value);
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

    // ── Helper ───────────────────────────────────────────────────────────────
    private async Task<Guid?> GetTeacherIdAsync()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return null;

        var teacher = await _unitOfWork.Teachers
            .FirstOrDefaultAsync(t => t.UserId == userId);

        return teacher?.Id;
    }
    // GET /api/marks/my-report-card — Student sees their own report card from JWT
    [HttpGet("my-report-card")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyReportCard()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var student = await _unitOfWork.Students
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student == null)
            return BadRequest(new { message = "Student profile not found." });

        try
        {
            var reportCard = await _marksService.GetReportCardAsync(student.Id);
            return Ok(reportCard);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolMgmt.DTOs.Timetable;
using SchoolMgmt.Repositories.Interfaces;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Controllers;

[ApiController]
[Route("api/timetable")]
[Authorize]
public class TimetableController : ControllerBase
{
    private readonly ITimetableService _timetableService;
    private readonly IUnitOfWork _unitOfWork;

    public TimetableController(
        ITimetableService timetableService,
        IUnitOfWork unitOfWork)
    {
        _timetableService = timetableService;
        _unitOfWork = unitOfWork;
    }

    // POST /api/timetable
    [HttpPost]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> CreateSlot([FromBody] CreateTimetableDto dto)
    {
        try
        {
            var result = await _timetableService.CreateSlotAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // GET /api/timetable/class/{classId}?academicYear=2024-25
    [HttpGet("class/{classId}")]
    public async Task<IActionResult> GetClassTimetable(
        Guid classId, [FromQuery] string academicYear = "2024-25")
    {
        var slots = await _timetableService
            .GetClassTimetableAsync(classId, academicYear);
        return Ok(slots);
    }

    // GET /api/timetable/my — teacher sees their own timetable
    [HttpGet("my")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetMyTimetable(
        [FromQuery] string academicYear = "2024-25")
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var teacher = await _unitOfWork.Teachers
            .FirstOrDefaultAsync(t => t.UserId == userId);

        if (teacher == null)
            return BadRequest(new { message = "Teacher profile not found." });

        var slots = await _timetableService
            .GetTeacherTimetableAsync(teacher.Id, academicYear);
        return Ok(slots);
    }

    // DELETE /api/timetable/{id}
    [HttpDelete("{timetableId}")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> DeleteSlot(Guid timetableId)
    {
        try
        {
            await _timetableService.DeleteSlotAsync(timetableId);
            return Ok(new { message = "Timetable slot deleted." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
    // GET /api/timetable/my-class — Student gets their own class timetable
    [HttpGet("my-class")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyClassTimetable(
        [FromQuery] string academicYear = "2024-25")
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var student = await _unitOfWork.Students
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student == null)
            return BadRequest(new { message = "Student profile not found." });

        var slots = await _timetableService
            .GetClassTimetableAsync(student.ClassId, academicYear);
        return Ok(slots);
    }
}
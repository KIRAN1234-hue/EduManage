using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolMgmt.DTOs.ExamSchedule;
using SchoolMgmt.Enums;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Controllers;

[ApiController]
[Route("api/exam-schedule")]
[Authorize]
public class ExamScheduleController : ControllerBase
{
    private readonly IExamScheduleService _examScheduleService;

    public ExamScheduleController(IExamScheduleService examScheduleService)
    {
        _examScheduleService = examScheduleService;
    }

    // POST /api/exam-schedule
    [HttpPost]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> CreateExam([FromBody] CreateExamScheduleDto dto)
    {
        try
        {
            var result = await _examScheduleService.CreateExamAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // GET /api/exam-schedule/class/{classId}?examType=Final
    [HttpGet("class/{classId}")]
    public async Task<IActionResult> GetClassExams(
        Guid classId,
        [FromQuery] ExamType? examType = null)
    {
        var exams = await _examScheduleService
            .GetClassExamsAsync(classId, examType);
        return Ok(exams);
    }

    // GET /api/exam-schedule/upcoming
    [HttpGet("upcoming")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> GetUpcomingExams()
    {
        var exams = await _examScheduleService.GetAllUpcomingExamsAsync();
        return Ok(exams);
    }

    // PUT /api/exam-schedule/{id}/toggle-marks-entry
    [HttpPut("{examScheduleId}/toggle-marks-entry")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> ToggleMarksEntry(Guid examScheduleId)
    {
        try
        {
            await _examScheduleService.ToggleMarksEntryAsync(examScheduleId);
            return Ok(new { message = "Marks entry status updated." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // DELETE /api/exam-schedule/{id}
    [HttpDelete("{examScheduleId}")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> DeleteExam(Guid examScheduleId)
    {
        try
        {
            await _examScheduleService.DeleteExamAsync(examScheduleId);
            return Ok(new { message = "Exam schedule deleted." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
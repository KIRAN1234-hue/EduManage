using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolMgmt.DTOs.Assignment;
using SchoolMgmt.Repositories.Interfaces;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Controllers;

[ApiController]
[Route("api/assignments")]
[Authorize]
public class AssignmentController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;
    private readonly IUnitOfWork _unitOfWork;

    public AssignmentController(
        IAssignmentService assignmentService,
        IUnitOfWork unitOfWork)
    {
        _assignmentService = assignmentService;
        _unitOfWork = unitOfWork;
    }

    // POST /api/assignments
    [HttpPost]
    [Authorize(Roles = "Teacher,Principal")]
    public async Task<IActionResult> CreateAssignment(
        [FromBody] CreateAssignmentDto dto)
    {
        var teacherId = await GetTeacherIdAsync();
        if (teacherId == null)
            return BadRequest(new { message = "Teacher profile not found." });

        try
        {
            var result = await _assignmentService.CreateAssignmentAsync(dto, teacherId.Value);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /api/assignments/class/{classId}
    [HttpGet("class/{classId}")]
    [Authorize(Roles = "Student,Teacher,Principal,Parent")]
    public async Task<IActionResult> GetClassAssignments(Guid classId)
    {
        var assignments = await _assignmentService.GetClassAssignmentsAsync(classId);
        return Ok(assignments);
    }

    // GET /api/assignments/my-assignments (teacher)
    [HttpGet("my-assignments")]
    [Authorize(Roles = "Teacher,Principal")]
    public async Task<IActionResult> GetMyAssignments()
    {
        var teacherId = await GetTeacherIdAsync();
        if (teacherId == null)
            return BadRequest(new { message = "Teacher profile not found." });

        var assignments = await _assignmentService.GetTeacherAssignmentsAsync(teacherId.Value);
        return Ok(assignments);
    }

    // GET /api/assignments/{id}
    [HttpGet("{assignmentId}")]
    public async Task<IActionResult> GetAssignment(Guid assignmentId)
    {
        try
        {
            var assignment = await _assignmentService.GetAssignmentByIdAsync(assignmentId);
            return Ok(assignment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // POST /api/assignments/{id}/submit
    [HttpPost("{assignmentId}/submit")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> SubmitAssignment(
        Guid assignmentId, [FromBody] SubmitAssignmentDto dto)
    {
        var studentId = await GetStudentIdAsync();
        if (studentId == null)
            return BadRequest(new { message = "Student profile not found." });

        try
        {
            var result = await _assignmentService
                .SubmitAssignmentAsync(assignmentId, studentId.Value, dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // GET /api/assignments/{id}/submissions
    [HttpGet("{assignmentId}/submissions")]
    [Authorize(Roles = "Teacher,Principal")]
    public async Task<IActionResult> GetSubmissions(Guid assignmentId)
    {
        var submissions = await _assignmentService.GetSubmissionsAsync(assignmentId);
        return Ok(submissions);
    }

    // PUT /api/assignments/submissions/{id}/grade
    [HttpPut("submissions/{submissionId}/grade")]
    [Authorize(Roles = "Teacher,Principal")]
    public async Task<IActionResult> GradeSubmission(
        Guid submissionId, [FromBody] GradeSubmissionDto dto)
    {
        try
        {
            var result = await _assignmentService.GradeSubmissionAsync(submissionId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // GET /api/assignments/{id}/my-submission
    [HttpGet("{assignmentId}/my-submission")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMySubmission(Guid assignmentId)
    {
        var studentId = await GetStudentIdAsync();
        if (studentId == null)
            return BadRequest(new { message = "Student profile not found." });

        var submission = await _assignmentService
            .GetMySubmissionAsync(assignmentId, studentId.Value);

        return Ok(submission);
    }

    private async Task<Guid?> GetTeacherIdAsync()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return null;
        var teacher = await _unitOfWork.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
        return teacher?.Id;
    }

    private async Task<Guid?> GetStudentIdAsync()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return null;
        var student = await _unitOfWork.Students.FirstOrDefaultAsync(s => s.UserId == userId);
        return student?.Id;
    }
    // GET /api/assignments/my-class — Student sees their class assignments
    [HttpGet("my-class")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyClassAssignments()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId))
            return Unauthorized();

        var student = await _unitOfWork.Students
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (student == null)
            return BadRequest(new { message = "Student profile not found." });

        var assignments = await _assignmentService
            .GetClassAssignmentsAsync(student.ClassId);

        return Ok(assignments);
    }
}
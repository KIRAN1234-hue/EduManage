using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolMgmt.DTOs.Leave;
using SchoolMgmt.Repositories.Interfaces;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Controllers;

[ApiController]
[Route("api/leave")]
[Authorize]
public class LeaveController : ControllerBase
{
    private readonly ILeaveService _leaveService;
    private readonly IUnitOfWork _unitOfWork;

    public LeaveController(ILeaveService leaveService, IUnitOfWork unitOfWork)
    {
        _leaveService = leaveService;
        _unitOfWork = unitOfWork;
    }

    // POST /api/leave  — only Student can apply
    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> ApplyLeave([FromBody] CreateLeaveDto dto)
    {
        var studentId = await GetStudentIdAsync();
        if (studentId == null)
            return BadRequest(new { message = "Student profile not found." });

        try
        {
            var result = await _leaveService.ApplyLeaveAsync(dto, studentId.Value);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /api/leave/my
    [HttpGet("my")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyLeaves()
    {
        var studentId = await GetStudentIdAsync();
        if (studentId == null)
            return BadRequest(new { message = "Student profile not found." });

        var leaves = await _leaveService.GetMyLeavesAsync(studentId.Value);
        return Ok(leaves);
    }

    // GET /api/leave/pending
    [HttpGet("pending")]
    [Authorize(Roles = "Principal,Teacher")]
    public async Task<IActionResult> GetPendingLeaves()
    {
        var leaves = await _leaveService.GetPendingLeavesAsync();
        return Ok(leaves);
    }

    // PUT /api/leave/{id}/process
    [HttpPut("{leaveId}/process")]
    [Authorize(Roles = "Principal,Teacher")]
    public async Task<IActionResult> ProcessLeave(
        Guid leaveId, [FromBody] ApproveLeaveDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var result = await _leaveService
                .ApproveOrRejectLeaveAsync(leaveId, dto, userId.Value);
            return Ok(result);
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

    private async Task<Guid?> GetStudentIdAsync()
    {
        var s = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(s, out var userId)) return null;
        var student = await _unitOfWork.Students
            .FirstOrDefaultAsync(st => st.UserId == userId);
        return student?.Id;
    }

    private Guid? GetUserId()
    {
        var s = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(s, out var id) ? id : null;
    }
}
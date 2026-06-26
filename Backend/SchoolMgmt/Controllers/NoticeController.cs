using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolMgmt.DTOs.Notice;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Controllers;

[ApiController]
[Route("api/notices")]
[Authorize]
public class NoticeController : ControllerBase
{
    private readonly INoticeService _noticeService;

    public NoticeController(INoticeService noticeService)
    {
        _noticeService = noticeService;
    }

    // POST /api/notices
    [HttpPost]
    [Authorize(Roles = "Principal,Teacher")]
    public async Task<IActionResult> CreateNotice([FromBody] CreateNoticeDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _noticeService.CreateNoticeAsync(dto, userId.Value);
        return Ok(result);
    }

    // GET /api/notices
    [HttpGet]
    public async Task<IActionResult> GetNotices()
    {
        var userId = GetUserId();
        var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "Student";
        if (userId == null) return Unauthorized();

        var notices = await _noticeService.GetNoticesAsync(userRole, userId.Value);
        return Ok(notices);
    }

    // POST /api/notices/{id}/acknowledge
    [HttpPost("{noticeId}/acknowledge")]
    public async Task<IActionResult> AcknowledgeNotice(Guid noticeId)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        await _noticeService.AcknowledgeNoticeAsync(noticeId, userId.Value);
        return Ok(new { message = "Notice acknowledged." });
    }

    // PUT /api/notices/{id}/archive
    [HttpPut("{noticeId}/archive")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> ArchiveNotice(Guid noticeId)
    {
        try
        {
            await _noticeService.ArchiveNoticeAsync(noticeId);
            return Ok(new { message = "Notice archived." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // DELETE /api/notices/{id}
    [HttpDelete("{noticeId}")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> DeleteNotice(Guid noticeId)
    {
        try
        {
            await _noticeService.DeleteNoticeAsync(noticeId);
            return Ok(new { message = "Notice deleted." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private Guid? GetUserId()
    {
        var s = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(s, out var id) ? id : null;
    }
}
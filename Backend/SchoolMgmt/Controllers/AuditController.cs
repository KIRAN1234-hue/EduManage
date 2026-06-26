using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolMgmt.Data;

namespace SchoolMgmt.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize(Roles = "Principal")]
public class AuditController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuditController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/audit?page=1&pageSize=50&entity=Attendance&action=Create
    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? entity = null,
        [FromQuery] string? action = null)
    {
        var query = _context.AuditLogs
            .Include(a => a.User)    // ✅ use navigation property
            .AsQueryable();

        if (!string.IsNullOrEmpty(entity))
            query = query.Where(a => a.EntityName == entity);

        if (!string.IsNullOrEmpty(action))
            query = query.Where(a => a.Action == action);

        var total = await query.CountAsync();

        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.Id,
                UserName = a.User.FullName,   // from navigation ✅
                UserEmail = a.User.Email,       // from navigation ✅
                a.Action,
                a.EntityName,
                a.EntityId,
                a.NewValues,
                a.OldValues,
                a.IPAddress,
                a.Timestamp
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, data = logs });
    }
}
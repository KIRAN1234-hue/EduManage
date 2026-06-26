using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolMgmt.Data;
using SchoolMgmt.DTOs.Complaint;
using SchoolMgmt.Entities;
using SchoolMgmt.Enums;
using SchoolMgmt.Repositories.Interfaces;

namespace SchoolMgmt.Controllers;

[ApiController]
[Route("api/complaints")]
[Authorize]
public class ComplaintController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;

    public ComplaintController(IUnitOfWork unitOfWork, AppDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    private Guid GetUserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id : Guid.Empty;

    // POST /api/complaints
    [HttpPost]
    [Authorize(Roles = "Student,Teacher,Parent")]
    public async Task<IActionResult> SubmitComplaint([FromBody] CreateComplaintDto dto)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var complaint = new Complaint
        {
            Id = Guid.NewGuid(),
            SubmittedByUserId = userId,
            //Title = dto.Title,
            Category = dto.Category,
            Description = dto.Description,
            Status = ComplaintStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Complaints.AddAsync(complaint);
        await _unitOfWork.SaveChangesAsync();

        return Ok(await BuildResponse(complaint.Id));
    }

    // GET /api/complaints/mine
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyComplaints()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var complaints = await _context.Complaints
            .Include(c => c.SubmittedBy)
            .Include(c => c.AssignedTo)
            .Where(c => c.SubmittedByUserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Ok(complaints.Select(MapComplaint));
    }

    // GET /api/complaints/all — Principal sees all
    [HttpGet("all")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> GetAllComplaints([FromQuery] string? status = null)
    {
        var query = _context.Complaints
            .Include(c => c.SubmittedBy)
            .Include(c => c.AssignedTo)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) &&
            Enum.TryParse<ComplaintStatus>(status, out var s))
            query = query.Where(c => c.Status == s);

        var complaints = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Ok(complaints.Select(MapComplaint));
    }

    // PUT /api/complaints/{id}/update — Principal updates status
    [HttpPut("{complaintId}/update")]
    [Authorize(Roles = "Principal")]
    public async Task<IActionResult> UpdateComplaint(
        Guid complaintId,
        [FromBody] UpdateComplaintDto dto)
    {
        var complaint = await _unitOfWork.Complaints.GetByIdAsync(complaintId);
        if (complaint == null) return NotFound();

        if (!string.IsNullOrEmpty(dto.NewStatus) &&
            Enum.TryParse<ComplaintStatus>(dto.NewStatus, out var newStatus))
        {
            // State machine: Open → InReview → Resolved (no skipping)
            if (complaint.Status == ComplaintStatus.Open &&
                newStatus == ComplaintStatus.Resolved)
                return BadRequest(new { message = "Cannot skip InReview state." });

            complaint.Status = newStatus;
            if (newStatus == ComplaintStatus.Resolved)
                complaint.ResolvedAt = DateTime.UtcNow;
        }

        if (dto.AssignedToUserId.HasValue)
            complaint.AssignedToUserId = dto.AssignedToUserId;

        if (!string.IsNullOrEmpty(dto.ResolutionRemark))
            complaint.ResolutionRemark = dto.ResolutionRemark;

        _unitOfWork.Complaints.Update(complaint);
        await _unitOfWork.SaveChangesAsync();

        return Ok(await BuildResponse(complaintId));
    }

    private async Task<ComplaintResponseDto> BuildResponse(Guid id)
    {
        var c = await _context.Complaints
            .Include(c => c.SubmittedBy)
            .Include(c => c.AssignedTo)
            .FirstOrDefaultAsync(c => c.Id == id)!;
        return MapComplaint(c!);
    }

    private static ComplaintResponseDto MapComplaint(Complaint c) => new()
    {
        Id = c.Id,
        //Title = c.Title,
        Category = c.Category.ToString(),
        Description = c.Description,
        Status = c.Status.ToString(),
        SubmittedBy = c.SubmittedBy?.FullName ?? string.Empty,
        AssignedTo = c.AssignedTo?.FullName,
        ResolutionRemark = c.ResolutionRemark,
        CreatedAt = c.CreatedAt,
        ResolvedAt = c.ResolvedAt
    };
}
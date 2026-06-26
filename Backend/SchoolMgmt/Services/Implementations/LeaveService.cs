using Microsoft.EntityFrameworkCore;
using SchoolMgmt.Data;
using SchoolMgmt.DTOs.Leave;
using SchoolMgmt.Entities;
using SchoolMgmt.Enums;
using SchoolMgmt.Repositories.Interfaces;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Services.Implementations;

public class LeaveService : ILeaveService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;


    public LeaveService(IUnitOfWork unitOfWork, AppDbContext context, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<LeaveResponseDto> ApplyLeaveAsync(
        CreateLeaveDto dto, Guid studentId)
    {
        if (dto.ToDate < dto.FromDate)
            throw new InvalidOperationException(
                "To date cannot be before From date.");

        var student = await _context.Students
        .Include(s => s.User)
        .FirstOrDefaultAsync(s => s.Id == studentId)
        ?? throw new KeyNotFoundException("Student not found.");

        var leave = new LeaveApplication
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,            // StudentId — correct FK
            UserId = student.UserId,
            LeaveType = dto.LeaveType,
            FromDate = dto.FromDate,
            ToDate = dto.ToDate,
            Reason = dto.Reason,
            Remarks = dto.Remarks,
            Status = LeaveStatus.Pending,
            AppliedOn = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.LeaveApplications.AddAsync(leave);
       
            await _unitOfWork.SaveChangesAsync();

        return await BuildLeaveResponse(leave.Id);
    }

    public async Task<IEnumerable<LeaveResponseDto>> GetMyLeavesAsync(Guid studentId)
    {
        var leaves = await _context.LeaveApplications
            .Include(l => l.Student).ThenInclude(s => s.User)
            .Include(l => l.ApprovedByTeacher).ThenInclude(t => t.User)
            .Where(l => l.StudentId == studentId)
            .OrderByDescending(l => l.AppliedOn)
            .ToListAsync();

        return leaves.Select(MapToDto);
    }

    public async Task<IEnumerable<LeaveResponseDto>> GetPendingLeavesAsync()
    {
        var leaves = await _context.LeaveApplications
            .Include(l => l.Student).ThenInclude(s => s.User)
            .Include(l => l.Student).ThenInclude(s => s.Class)
            .Where(l => l.Status == LeaveStatus.Pending)
            .OrderBy(l => l.AppliedOn)
            .ToListAsync();

        return leaves.Select(MapToDto);
    }

    public async Task<LeaveResponseDto> ApproveOrRejectLeaveAsync(
        Guid leaveId, ApproveLeaveDto dto, Guid approvedByUserId)
    {
        var leave = await _unitOfWork.LeaveApplications.GetByIdAsync(leaveId)
            ?? throw new KeyNotFoundException("Leave application not found.");

        if (leave.Status != LeaveStatus.Pending)
            throw new InvalidOperationException("Leave is already processed.");

        // Find the teacher record for this user
        var teacher = await _unitOfWork.Teachers
            .FirstOrDefaultAsync(t => t.UserId == approvedByUserId);

        leave.Status = dto.IsApproved
            ? LeaveStatus.Approved
            : LeaveStatus.Rejected;
        leave.ApprovalRemark = dto.ApprovalRemark; // correct property name
        leave.ApprovedById = approvedByUserId;
        leave.ApprovedAt = DateTime.UtcNow;
        leave.ApprovedByTeacherId = teacher?.Id;         // correct property name

        _unitOfWork.LeaveApplications.Update(leave);
        await _unitOfWork.SaveChangesAsync();

        // Notify student when leave is approved or rejected
        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == leave.StudentId);

        //if (student?.User != null)
        //{
        //    var status = isApproved ? "Approved" : "Rejected";
        //    //await _notificationService.PushAsync(
        //    //    userId: student.UserId,
        //    //    title: $"Leave Application {status}",
        //    //    body: remark ?? $"Your leave request has been {status.ToLower()}.",
        //    //    type: "Leave",
        //    //    actionUrl: "/student/leave",
        //    //    relatedEntityId: leave.Id.ToString()
        //    //);
        //}

        await NotifyLeaveStatusAsync(leave.StudentId, dto.IsApproved, dto.ApprovalRemark);

        return await BuildLeaveResponse(leaveId);
    }

    private async Task<LeaveResponseDto> BuildLeaveResponse(Guid leaveId)
    {
        var l = await _context.LeaveApplications
            .Include(l => l.Student).ThenInclude(s => s.User)
            .Include(l => l.ApprovedByTeacher).ThenInclude(t => t.User)
            .FirstOrDefaultAsync(l => l.Id == leaveId)
            ?? throw new KeyNotFoundException("Leave not found.");

        return MapToDto(l);
    }

    private static LeaveResponseDto MapToDto(LeaveApplication l) => new()
    {
        Id = l.Id,
        StudentName = l.Student?.User?.FullName ?? string.Empty,
        RollNumber = l.Student?.RollNumber ?? string.Empty,
        LeaveType = l.LeaveType,
        FromDate = l.FromDate,
        ToDate = l.ToDate,
        TotalDays = l.ToDate.DayNumber - l.FromDate.DayNumber + 1,
        Reason = l.Reason,
        Remarks = l.Remarks,
        Status = l.Status.ToString(),
        ApprovalRemark = l.ApprovalRemark,           // correct property name
        ApprovedBy = l.ApprovedByTeacher?.User?.FullName
                      ?? l.ApprovedBy?.FullName,
        AppliedOn = l.AppliedOn
    };

    private async Task NotifyLeaveStatusAsync(Guid studentId, bool isApproved, string? remark)
    {
        try
        {
            var leaveStudent = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (leaveStudent?.User != null)
            {
                var statusText = isApproved ? "Approved" : "Rejected";
                await _notificationService.PushAsync(
                    userId: leaveStudent.UserId,
                    title: $"Leave Application {statusText}",
                    body: remark ?? $"Your leave request has been {statusText.ToLower()}.",
                    type: "Leave",
                    actionUrl: "/student/leave"
                );
            }
        }
        catch { }
    }
}
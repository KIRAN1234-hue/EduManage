using SchoolMgmt.DTOs.Leave;

namespace SchoolMgmt.Services.Interfaces;

public interface ILeaveService
{
    Task<LeaveResponseDto> ApplyLeaveAsync(
        CreateLeaveDto dto, Guid studentId);  // studentId — not userId

    Task<IEnumerable<LeaveResponseDto>> GetMyLeavesAsync(Guid studentId);

    Task<IEnumerable<LeaveResponseDto>> GetPendingLeavesAsync();

    Task<LeaveResponseDto> ApproveOrRejectLeaveAsync(
        Guid leaveId, ApproveLeaveDto dto, Guid approvedByUserId);
}
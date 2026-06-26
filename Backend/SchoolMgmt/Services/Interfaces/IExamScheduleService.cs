using SchoolMgmt.DTOs.ExamSchedule;
using SchoolMgmt.Enums;

namespace SchoolMgmt.Services.Interfaces;

public interface IExamScheduleService
{
    Task<ExamScheduleResponseDto> CreateExamAsync(CreateExamScheduleDto dto);
    Task<IEnumerable<ExamScheduleResponseDto>> GetClassExamsAsync(Guid classId, ExamType? examType = null);
    Task<IEnumerable<ExamScheduleResponseDto>> GetAllUpcomingExamsAsync();
    Task ToggleMarksEntryAsync(Guid examScheduleId);
    Task DeleteExamAsync(Guid examScheduleId);
}
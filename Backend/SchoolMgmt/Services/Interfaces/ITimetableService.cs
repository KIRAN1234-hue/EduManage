using SchoolMgmt.DTOs.Timetable;

namespace SchoolMgmt.Services.Interfaces;

public interface ITimetableService
{
    Task<TimetableResponseDto> CreateSlotAsync(CreateTimetableDto dto);
    Task<IEnumerable<TimetableResponseDto>> GetClassTimetableAsync(Guid classId, string academicYear);
    Task<IEnumerable<TimetableResponseDto>> GetTeacherTimetableAsync(Guid teacherId, string academicYear);
    Task DeleteSlotAsync(Guid timetableId);
}
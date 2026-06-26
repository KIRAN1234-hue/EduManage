using SchoolMgmt.DTOs.Admin;

namespace SchoolMgmt.Services.Interfaces;

public interface IAdminService
{
    // ── Classes ──────────────────────────────────────────────────────────────
    Task<ClassResponseDto> CreateClassAsync(CreateClassDto dto);
    Task<IEnumerable<ClassResponseDto>> GetAllClassesAsync();
    Task<ClassResponseDto> GetClassByIdAsync(Guid classId);

    // ── Subjects ─────────────────────────────────────────────────────────────
    Task<SubjectResponseDto> CreateSubjectAsync(CreateSubjectDto dto);
    Task<IEnumerable<SubjectResponseDto>> GetAllSubjectsAsync();
    Task<IEnumerable<SubjectResponseDto>> GetSubjectsByClassAsync(Guid classId);

    // ── Teachers ─────────────────────────────────────────────────────────────
    Task<TeacherResponseDto> CreateTeacherAsync(CreateTeacherDto dto);
    Task<IEnumerable<TeacherResponseDto>> GetAllTeachersAsync();

    // ── Students ─────────────────────────────────────────────────────────────
    Task<StudentResponseDto> CreateStudentAsync(CreateStudentDto dto);
    Task<IEnumerable<StudentResponseDto>> GetAllStudentsAsync();
    Task<IEnumerable<StudentResponseDto>> GetStudentsByClassAsync(Guid classId);

    // ── Deactivate user ───────────────────────────────────────────────────────
    Task DeactivateUserAsync(Guid userId);
    Task DeleteClassAsync(Guid classId);
    Task DeleteSubjectAsync(Guid subjectId);
    Task DeleteTeacherAsync(Guid teacherId);
    Task DeleteStudentAsync(Guid studentId);
    Task<List<TeacherResponseDto>> GetTeachersForDoubtsAsync();
}
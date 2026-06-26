using SchoolMgmt.DTOs.Assignment;

namespace SchoolMgmt.Services.Interfaces;

public interface IAssignmentService
{
    Task<AssignmentResponseDto> CreateAssignmentAsync(
        CreateAssignmentDto dto, Guid teacherId);

    Task<IEnumerable<AssignmentResponseDto>> GetClassAssignmentsAsync(Guid classId);

    Task<IEnumerable<AssignmentResponseDto>> GetTeacherAssignmentsAsync(Guid teacherId);

    Task<AssignmentResponseDto> GetAssignmentByIdAsync(Guid assignmentId);

    Task<SubmissionResponseDto> SubmitAssignmentAsync(
        Guid assignmentId, Guid studentId, SubmitAssignmentDto dto);

    Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsAsync(Guid assignmentId);

    Task<SubmissionResponseDto> GradeSubmissionAsync(
        Guid submissionId, GradeSubmissionDto dto);

    Task<SubmissionResponseDto?> GetMySubmissionAsync(
        Guid assignmentId, Guid studentId);
}
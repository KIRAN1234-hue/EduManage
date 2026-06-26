using SchoolMgmt.DTOs.Marks;

namespace SchoolMgmt.Services.Interfaces;

public interface IMarksService
{
    // Teacher submits marks for entire class in one batch
    Task<string> CreateBulkMarksAsync(
        List<CreateMarkDto> markDtos, Guid teacherId);

    // Get all marks for a student
    Task<IEnumerable<MarkResponseDto>> GetStudentMarksAsync(Guid studentId);

    // Get full report card for a student
    Task<ReportCardDto> GetReportCardAsync(Guid studentId);

    // Get class marks for a specific exam type — teacher view
    Task<IEnumerable<MarkResponseDto>> GetClassMarksAsync(
        Guid classId, string examType);

    // Get Chart.js compatible data for Angular bar chart
    Task<ChartDataDto> GetChartDataAsync(Guid studentId);

    // Teacher corrects an entered mark
    Task<MarkResponseDto> UpdateMarkAsync(
        Guid markId, UpdateMarkDto dto, Guid teacherId);
}
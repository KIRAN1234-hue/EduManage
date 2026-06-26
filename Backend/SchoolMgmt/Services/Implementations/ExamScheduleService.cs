using Microsoft.EntityFrameworkCore;
using SchoolMgmt.Data;
using SchoolMgmt.DTOs.ExamSchedule;
using SchoolMgmt.Entities;
using SchoolMgmt.Enums;
using SchoolMgmt.Repositories.Interfaces;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Services.Implementations;

public class ExamScheduleService : IExamScheduleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;

    public ExamScheduleService(IUnitOfWork unitOfWork, AppDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<ExamScheduleResponseDto> CreateExamAsync(CreateExamScheduleDto dto)
    {
        // Check no clash — same class, same date, overlapping time
        var dateClash = await _unitOfWork.ExamSchedules.AnyAsync(e =>
            e.ClassId == dto.ClassId &&
            e.ExamDate == dto.ExamDate &&
            e.ExamType == dto.ExamType);

        if (dateClash)
            throw new InvalidOperationException(
                "An exam of this type is already scheduled for this class on that date.");

        var exam = new ExamSchedule
        {
            Id = Guid.NewGuid(),
            SubjectId = dto.SubjectId,
            ClassId = dto.ClassId,
            InvigilatorTeacherId = dto.InvigilatorTeacherId,
            ExamType = dto.ExamType,
            ExamDate = dto.ExamDate,
            StartTime = TimeOnly.Parse(dto.StartTime),
            EndTime = TimeOnly.Parse(dto.EndTime),
            RoomNumber = dto.RoomNumber,
            MarksEntryOpen = false
        };

        await _unitOfWork.ExamSchedules.AddAsync(exam);
        await _unitOfWork.SaveChangesAsync();

        return await BuildResponse(exam.Id);
    }

    public async Task<IEnumerable<ExamScheduleResponseDto>> GetClassExamsAsync(
        Guid classId, ExamType? examType = null)
    {
        var query = _context.ExamSchedules
            .Include(e => e.Subject)
            .Include(e => e.Class)
            .Include(e => e.InvigilatorTeacher).ThenInclude(t => t.User)
            .Where(e => e.ClassId == classId);

        if (examType.HasValue)
            query = query.Where(e => e.ExamType == examType.Value);

        var exams = await query
            .OrderBy(e => e.ExamDate)
            .ThenBy(e => e.StartTime)
            .ToListAsync();

        return exams.Select(MapToDto);
    }

    public async Task<IEnumerable<ExamScheduleResponseDto>> GetAllUpcomingExamsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var exams = await _context.ExamSchedules
            .Include(e => e.Subject)
            .Include(e => e.Class)
            .Include(e => e.InvigilatorTeacher).ThenInclude(t => t.User)
            .Where(e => e.ExamDate >= today)
            .OrderBy(e => e.ExamDate)
            .ThenBy(e => e.StartTime)
            .ToListAsync();

        return exams.Select(MapToDto);
    }

    public async Task ToggleMarksEntryAsync(Guid examScheduleId)
    {
        var exam = await _unitOfWork.ExamSchedules.GetByIdAsync(examScheduleId)
            ?? throw new KeyNotFoundException("Exam schedule not found.");

        exam.MarksEntryOpen = !exam.MarksEntryOpen;
        _unitOfWork.ExamSchedules.Update(exam);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteExamAsync(Guid examScheduleId)
    {
        var exam = await _unitOfWork.ExamSchedules.GetByIdAsync(examScheduleId)
            ?? throw new KeyNotFoundException("Exam schedule not found.");

        _unitOfWork.ExamSchedules.Remove(exam);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<ExamScheduleResponseDto> BuildResponse(Guid id)
    {
        var e = await _context.ExamSchedules
            .Include(e => e.Subject)
            .Include(e => e.Class)
            .Include(e => e.InvigilatorTeacher).ThenInclude(t => t.User)
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new KeyNotFoundException("Exam schedule not found.");

        return MapToDto(e);
    }

    private static ExamScheduleResponseDto MapToDto(ExamSchedule e) => new()
    {
        Id = e.Id,
        SubjectName = e.Subject?.Name ?? string.Empty,
        ClassName = e.Class != null ? $"{e.Class.Name} {e.Class.Section}" : string.Empty,
        InvigilatorTeacher = e.InvigilatorTeacher?.User?.FullName ?? string.Empty,
        ExamType = e.ExamType.ToString(),
        ExamDate = e.ExamDate,
        StartTime = e.StartTime.ToString("HH:mm"),
        EndTime = e.EndTime.ToString("HH:mm"),
        RoomNumber = e.RoomNumber,
        MarksEntryOpen = e.MarksEntryOpen
    };
}
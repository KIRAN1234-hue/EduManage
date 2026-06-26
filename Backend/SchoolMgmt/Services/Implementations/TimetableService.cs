using Microsoft.EntityFrameworkCore;
using SchoolMgmt.Data;
using SchoolMgmt.DTOs.Timetable;
using SchoolMgmt.Entities;
using SchoolMgmt.Repositories.Interfaces;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Services.Implementations;

public class TimetableService : ITimetableService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;
    private readonly ICacheService _cacheService;

    public TimetableService(IUnitOfWork unitOfWork, AppDbContext context, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<TimetableResponseDto> CreateSlotAsync(CreateTimetableDto dto)
    {
        // Check no clash — same class, same day, same period
        var clashExists = await _unitOfWork.Timetables.AnyAsync(t =>
            t.ClassId == dto.ClassId &&
            t.DayOfWeek == dto.DayOfWeek &&
            t.PeriodNumber == dto.PeriodNumber &&
            t.AcademicYear == dto.AcademicYear);

        if (clashExists)
            throw new InvalidOperationException(
                $"Period {dto.PeriodNumber} on {dto.DayOfWeek} is already assigned for this class.");

        var slot = new Timetable
        {
            Id = Guid.NewGuid(),
            ClassId = dto.ClassId,
            SubjectId = dto.SubjectId,
            TeacherId = dto.TeacherId,
            DayOfWeek = dto.DayOfWeek,
            PeriodNumber = dto.PeriodNumber,
            StartTime = TimeOnly.Parse(dto.StartTime),
            EndTime = TimeOnly.Parse(dto.EndTime),
            AcademicYear = dto.AcademicYear
        };

        await _unitOfWork.Timetables.AddAsync(slot);
        await _unitOfWork.SaveChangesAsync();

        return await BuildResponse(slot.Id);
    }

    public async Task<IEnumerable<TimetableResponseDto>> GetClassTimetableAsync(
    Guid classId, string academicYear)
    {
        var cacheKey = CacheKeys.ClassTimetable(classId, academicYear);

        // Try cache first
        var cached = await _cacheService.GetAsync<List<TimetableResponseDto>>(cacheKey);
        if (cached != null) return cached;

        // Cache miss — query DB
        var slots = await _context.Timetables
            .Include(t => t.Class)
            .Include(t => t.Subject)
            .Include(t => t.Teacher).ThenInclude(t => t.User)
            .Where(t => t.ClassId == classId && t.AcademicYear == academicYear)
            .OrderBy(t => t.DayOfWeek).ThenBy(t => t.PeriodNumber)
            .ToListAsync();

        var result = slots.Select(MapToDto).ToList();

        // Store in cache — 1 hour TTL
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(1));

        return result;
    }

    public async Task<IEnumerable<TimetableResponseDto>> GetTeacherTimetableAsync(
        Guid teacherId, string academicYear)
    {
        var slots = await _context.Timetables
            .Include(t => t.Class)
            .Include(t => t.Subject)
            .Include(t => t.Teacher).ThenInclude(t => t.User)
            .Where(t => t.TeacherId == teacherId &&
                        t.AcademicYear == academicYear)
            .OrderBy(t => t.DayOfWeek)
            .ThenBy(t => t.PeriodNumber)
            .ToListAsync();

        return slots.Select(MapToDto);
    }

    public async Task DeleteSlotAsync(Guid timetableId)
    {
        var slot = await _unitOfWork.Timetables.GetByIdAsync(timetableId)
            ?? throw new KeyNotFoundException("Timetable slot not found.");

        _unitOfWork.Timetables.Remove(slot);
        await _unitOfWork.SaveChangesAsync();

        // Invalidate timetable cache
        await _cacheService.RemoveByPrefixAsync(CacheKeys.TimetablePrefix);
    }

    private async Task<TimetableResponseDto> BuildResponse(Guid id)
    {
        var t = await _context.Timetables
            .Include(t => t.Class)
            .Include(t => t.Subject)
            .Include(t => t.Teacher).ThenInclude(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException("Timetable slot not found.");

        return MapToDto(t);
    }

    private static TimetableResponseDto MapToDto(Timetable t) => new()
    {
        Id = t.Id,
        ClassName = t.Class != null ? $"{t.Class.Name} {t.Class.Section}" : string.Empty,
        SubjectName = t.Subject?.Name ?? string.Empty,
        TeacherName = t.Teacher?.User?.FullName ?? string.Empty,
        DayOfWeek = t.DayOfWeek.ToString(),
        PeriodNumber = t.PeriodNumber,
        StartTime = t.StartTime.ToString("HH:mm"),
        EndTime = t.EndTime.ToString("HH:mm"),
        AcademicYear = t.AcademicYear
    };
}
using SchoolMgmt.Data;
using SchoolMgmt.DTOs.Marks;
using SchoolMgmt.Entities;
using SchoolMgmt.Enums;
using SchoolMgmt.Repositories.Interfaces;
using SchoolMgmt.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SchoolMgmt.Services.Implementations;

public class MarksService : IMarksService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly AppDbContext _context;

    public MarksService(IUnitOfWork unitOfWork, INotificationService notificationService, AppDbContext context)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _context = context;
    }

    // ── Create Bulk Marks ────────────────────────────────────────────────────
    public async Task<string> CreateBulkMarksAsync(
        List<CreateMarkDto> markDtos, Guid teacherId)
    {
        var created = 0;
        var skipped = 0;

        foreach (var dto in markDtos)
        {
            // Skip if already entered — prevent duplicates
            var alreadyEntered = await _unitOfWork.Marks
                .IsAlreadyEnteredAsync(dto.StudentId, dto.SubjectId, dto.ExamType);

            if (alreadyEntered) { skipped++; continue; }

            if (!Enum.TryParse<ExamType>(dto.ExamType, ignoreCase: true, out var examType))
                continue; // skip invalid exam type

            var percentage = GradeService.CalculatePercentage(dto.MarksObtained, dto.MaxMarks);
            var grade = GradeService.CalculateGrade(percentage);

            var mark = new Mark
            {
                Id = Guid.NewGuid(),
                StudentId = dto.StudentId,
                SubjectId = dto.SubjectId,
                EnteredByTeacherId = teacherId,
                ExamType = examType,
                MarksObtained = dto.MarksObtained,
                MaxMarks = dto.MaxMarks,
                Grade = grade,
                EnteredAt = DateTime.UtcNow
            };

            await _unitOfWork.Marks.AddAsync(mark);
            created++;
        }

        await _unitOfWork.SaveChangesAsync();

        // Notify student when marks are entered
        //await _notificationService.PushAsync(
        //    userId: student.UserId,
        //    title: $"Marks Published: {subject?.Name}",
        //    body: $"{examType} marks have been published.",
        //    type: "Mark",
        //    actionUrl: "/student/marks",
        //    relatedEntityId: student.Id.ToString()
        //);


        return $"{created} marks saved. {skipped} skipped (already entered).";
    }

    // ── Get Student Marks ────────────────────────────────────────────────────
    public async Task<IEnumerable<MarkResponseDto>> GetStudentMarksAsync(
        Guid studentId)
    {
        var marks = await _unitOfWork.Marks.GetStudentMarksAsync(studentId);

        return marks.Select(m => new MarkResponseDto
        {
            Id = m.Id,
            StudentId = m.StudentId,
            StudentName = m.Student?.User?.FullName ?? string.Empty,
            RollNumber = m.Student?.RollNumber ?? string.Empty,
            SubjectId = m.SubjectId,
            SubjectName = m.Subject?.Name ?? string.Empty,
            ExamType = m.ExamType.ToString(),
            MarksObtained = m.MarksObtained,
            MaxMarks = m.MaxMarks,
            Percentage = GradeService.CalculatePercentage(m.MarksObtained, m.MaxMarks),
            Grade = m.Grade,
            EnteredAt = m.EnteredAt,
            EnteredByTeacher = m.EnteredByTeacher?.User?.FullName ?? string.Empty
        });
    }

    // ── Get Report Card ──────────────────────────────────────────────────────
    public async Task<ReportCardDto> GetReportCardAsync(Guid studentId)
    {
        var student = await _unitOfWork.Students.GetByIdAsync(studentId)
            ?? throw new KeyNotFoundException($"Student {studentId} not found.");

        // Load student with user and class
        var studentWithDetails = await _unitOfWork.Students
            .FirstOrDefaultAsync(s => s.Id == studentId);

        var marks = await _unitOfWork.Marks.GetStudentMarksAsync(studentId);
        var markList = marks.ToList();

        // Group marks by subject
        var subjectGroups = markList
            .GroupBy(m => new { m.SubjectId, m.Subject?.Name, m.Subject?.Code });

        var subjectReports = new List<SubjectReportDto>();

        foreach (var group in subjectGroups)
        {
            var examMarks = group.Select(m => new ExamMarkDto
            {
                ExamType = m.ExamType.ToString(),
                MarksObtained = m.MarksObtained,
                MaxMarks = m.MaxMarks,
                Grade = m.Grade
            }).ToList();

            // Subject percentage = average across all exam types
            var subjectPercentage = group.Any()
                ? group.Average(m =>
                    GradeService.CalculatePercentage(m.MarksObtained, m.MaxMarks))
                : 0;

            subjectReports.Add(new SubjectReportDto
            {
                SubjectName = group.Key.Name ?? string.Empty,
                SubjectCode = group.Key.Code ?? string.Empty,
                Exams = examMarks,
                SubjectPercentage = Math.Round((decimal)subjectPercentage, 2),
                SubjectGrade = GradeService.CalculateGrade((decimal)subjectPercentage)
            });
        }

        // Overall percentage = average across all marks
        var overallPercentage = markList.Any()
            ? markList.Average(m =>
                (double)GradeService.CalculatePercentage(m.MarksObtained, m.MaxMarks))
            : 0;

        return new ReportCardDto
        {
            StudentId = studentId,
            StudentName = markList.FirstOrDefault()?.Student?.User?.FullName ?? string.Empty,
            RollNumber = markList.FirstOrDefault()?.Student?.RollNumber ?? string.Empty,
            ClassName = markList.FirstOrDefault()?.Student?.Class?.Name ?? string.Empty,
            AcademicYear = markList.FirstOrDefault()?.Student?.Class?.AcademicYear ?? string.Empty,
            OverallPercentage = Math.Round((decimal)overallPercentage, 2),
            OverallGrade = GradeService.CalculateGrade((decimal)overallPercentage),
            Subjects = subjectReports.OrderBy(s => s.SubjectName).ToList()
        };
    }

    // ── Get Class Marks ──────────────────────────────────────────────────────
    public async Task<IEnumerable<MarkResponseDto>> GetClassMarksAsync(
        Guid classId, string examType)
    {
        var marks = await _unitOfWork.Marks.GetClassMarksAsync(classId, examType);

        return marks.Select(m => new MarkResponseDto
        {
            Id = m.Id,
            StudentId = m.StudentId,
            StudentName = m.Student?.User?.FullName ?? string.Empty,
            RollNumber = m.Student?.RollNumber ?? string.Empty,
            SubjectId = m.SubjectId,
            SubjectName = m.Subject?.Name ?? string.Empty,
            ExamType = m.ExamType.ToString(),
            MarksObtained = m.MarksObtained,
            MaxMarks = m.MaxMarks,
            Percentage = GradeService.CalculatePercentage(m.MarksObtained, m.MaxMarks),
            Grade = m.Grade,
            EnteredAt = m.EnteredAt
        });
    }

    // ── Get Chart Data ───────────────────────────────────────────────────────
    public async Task<ChartDataDto> GetChartDataAsync(Guid studentId)
    {
        var marks = await _unitOfWork.Marks
            .GetStudentMarksWithDetailsAsync(studentId);

        var markList = marks.ToList();

        // Labels = subject names
        var subjects = markList
            .Select(m => m.Subject?.Name ?? string.Empty)
            .Distinct()
            .OrderBy(s => s)
            .ToList();

        // Datasets = one per exam type
        var examTypes = markList
            .Select(m => m.ExamType.ToString())
            .Distinct()
            .ToList();

        var colors = new[]
        {
            "#2563EB", "#16A34A", "#D97706", "#DC2626",
            "#7C3AED", "#0D9488", "#EA580C"
        };

        var datasets = examTypes.Select((examType, index) =>
        {
            var data = subjects.Select(subject =>
            {
                var mark = markList.FirstOrDefault(m =>
                    m.Subject?.Name == subject &&
                    m.ExamType.ToString() == examType);

                return mark != null
                    ? GradeService.CalculatePercentage(mark.MarksObtained, mark.MaxMarks)
                    : 0m;
            }).ToList();

            return new ChartDatasetDto
            {
                Label = examType,
                Data = data,
                BackgroundColor = colors[index % colors.Length]
            };
        }).ToList();

        return new ChartDataDto
        {
            Labels = subjects,
            Datasets = datasets
        };
    }

    // ── Update Mark ──────────────────────────────────────────────────────────
    public async Task<MarkResponseDto> UpdateMarkAsync(
        Guid markId, UpdateMarkDto dto, Guid teacherId)
    {
        var mark = await _unitOfWork.Marks.GetByIdAsync(markId)
            ?? throw new KeyNotFoundException($"Mark {markId} not found.");

        if (mark.EnteredByTeacherId != teacherId)
            throw new UnauthorizedAccessException(
                "You can only edit marks you entered.");

        mark.MarksObtained = dto.MarksObtained;
        mark.Grade = GradeService.CalculateGrade(
            GradeService.CalculatePercentage(dto.MarksObtained, mark.MaxMarks));

        _unitOfWork.Marks.Update(mark);
        await _unitOfWork.SaveChangesAsync();

        return new MarkResponseDto
        {
            Id = mark.Id,
            StudentId = mark.StudentId,
            SubjectId = mark.SubjectId,
            ExamType = mark.ExamType.ToString(),
            MarksObtained = mark.MarksObtained,
            MaxMarks = mark.MaxMarks,
            Percentage = GradeService.CalculatePercentage(
                                mark.MarksObtained, mark.MaxMarks),
            Grade = mark.Grade,
            EnteredAt = mark.EnteredAt
        };
    }

    private async Task NotifyStudentMarksAsync(Guid studentId)
    {
        try
        {
            var marksStudent = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (marksStudent?.User != null)
            {
                await _notificationService.PushAsync(
                    userId: marksStudent.UserId,
                    title: "Marks Published",
                    body: "Your exam marks have been published. Check your report card.",
                    type: "Mark",
                    actionUrl: "/student/marks"
                );
            }
        }
        catch { /* Notification failure must not break main flow */ }
    }
}
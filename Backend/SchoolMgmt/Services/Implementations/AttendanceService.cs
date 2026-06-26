using SchoolMgmt.DTOs.Attendance;
using SchoolMgmt.Entities;
using SchoolMgmt.Enums;
using SchoolMgmt.Repositories.Interfaces;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Services.Implementations;

public class AttendanceService : IAttendanceService
{
    private readonly IUnitOfWork _unitOfWork;

    public AttendanceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // ── Mark Bulk Attendance ─────────────────────────────────────────────────
    public async Task<string> MarkBulkAttendanceAsync(
    List<MarkAttendanceDto> attendanceDtos, Guid? teacherId)
    {
        var marked = 0;
        var skipped = 0;

        foreach (var dto in attendanceDtos)
        {
            // Skip if already marked — prevent duplicate attendance
            var alreadyMarked = await _unitOfWork.Attendances
                .IsAlreadyMarkedAsync(dto.StudentId, dto.SubjectId, dto.Date);

            if (alreadyMarked)
            {
                skipped++;
                continue;
            }

            // Parse status string to enum
            if (!Enum.TryParse<AttendanceStatus>(dto.Status, ignoreCase: true, out var status))
                status = AttendanceStatus.Present; // default to Present if invalid

            var attendance = new Attendance
            {
                Id = Guid.NewGuid(),
                StudentId = dto.StudentId,
                SubjectId = dto.SubjectId,
                MarkedByTeacherId = teacherId,
                Date = dto.Date,
                Status = status,
                Remarks = dto.Remarks
            };

            await _unitOfWork.Attendances.AddAsync(attendance);
            marked++;
        }

        // Save all in one transaction
        await _unitOfWork.SaveChangesAsync();

        return $"{marked} attendance records saved. {skipped} skipped (already marked).";
    }

    // ── Get Student Attendance ───────────────────────────────────────────────
    public async Task<IEnumerable<AttendanceResponseDto>> GetStudentAttendanceAsync(
        Guid studentId)
    {
        var records = await _unitOfWork.Attendances
            .GetStudentAttendanceAsync(studentId);

        return records.Select(a => new AttendanceResponseDto
        {
            Id = a.Id,
            StudentId = a.StudentId,
            StudentName = a.Student?.User?.FullName ?? string.Empty,
            RollNumber = a.Student?.RollNumber ?? string.Empty,
            SubjectId = a.SubjectId,
            SubjectName = a.Subject?.Name ?? string.Empty,
            Date = a.Date,
            Status = a.Status.ToString(),
            Remarks = a.Remarks,
            MarkedByTeacher = a.MarkedByTeacher?.User?.FullName ?? string.Empty
        });
    }

    // ── Get Attendance Percentage Per Subject ────────────────────────────────
    public async Task<IEnumerable<AttendancePercentageDto>> GetAttendancePercentageAsync(
        Guid studentId)
    {
        // Get all subjects for this student's class
        var student = await _unitOfWork.Students.GetByIdAsync(studentId);
        if (student is null)
            throw new KeyNotFoundException($"Student {studentId} not found.");

        var subjects = await _unitOfWork.Subjects
            .FindAsync(s => s.ClassId == student.ClassId);

        var result = new List<AttendancePercentageDto>();

        foreach (var subject in subjects)
        {
            var records = await _unitOfWork.Attendances
                .GetStudentSubjectAttendanceAsync(studentId, subject.Id);

            var recordList = records.ToList();
            var total = recordList.Count;
            var present = recordList.Count(r => r.Status == AttendanceStatus.Present);
            var late = recordList.Count(r => r.Status == AttendanceStatus.Late);
            var absent = recordList.Count(r => r.Status == AttendanceStatus.Absent);

            // Present + Late both count toward attendance
            var percentage = total == 0 ? 0
                : Math.Round((decimal)(present + late) / total * 100, 2);

            result.Add(new AttendancePercentageDto
            {
                SubjectId = subject.Id,
                SubjectName = subject.Name,
                TotalClasses = total,
                PresentCount = present,
                AbsentCount = absent,
                LateCount = late,
                Percentage = percentage
            });
        }

        return result.OrderByDescending(r => r.Percentage);
    }

    // ── Get Class Attendance For Date ────────────────────────────────────────
    public async Task<IEnumerable<AttendanceResponseDto>> GetClassAttendanceForDateAsync(
        Guid classId, DateOnly date)
    {
        var records = await _unitOfWork.Attendances
            .GetClassAttendanceForDateAsync(classId, date);

        return records.Select(a => new AttendanceResponseDto
        {
            Id = a.Id,
            StudentId = a.StudentId,
            StudentName = a.Student?.User?.FullName ?? string.Empty,
            RollNumber = a.Student?.RollNumber ?? string.Empty,
            SubjectId = a.SubjectId,
            SubjectName = a.Subject?.Name ?? string.Empty,
            Date = a.Date,
            Status = a.Status.ToString(),
            Remarks = a.Remarks,
            MarkedByTeacher = a.MarkedByTeacher?.User?.FullName ?? string.Empty
        });
    }

    // ── Update Attendance ────────────────────────────────────────────────────
    public async Task<AttendanceResponseDto> UpdateAttendanceAsync(
        Guid attendanceId, UpdateAttendanceDto dto, Guid? teacherId)
    {
        var attendance = await _unitOfWork.Attendances.GetByIdAsync(attendanceId);

        if (attendance is null)
            throw new KeyNotFoundException($"Attendance record {attendanceId} not found.");

        // Only the teacher who marked it can correct it
        if (attendance.MarkedByTeacherId != teacherId)
            throw new UnauthorizedAccessException(
                "You can only edit attendance records you marked.");

        // Update status
        if (Enum.TryParse<AttendanceStatus>(dto.Status, ignoreCase: true, out var status))
            attendance.Status = status;

        attendance.Remarks = dto.Remarks;

        _unitOfWork.Attendances.Update(attendance);
        await _unitOfWork.SaveChangesAsync();

        return new AttendanceResponseDto
        {
            Id = attendance.Id,
            StudentId = attendance.StudentId,
            SubjectId = attendance.SubjectId,
            Date = attendance.Date,
            Status = attendance.Status.ToString(),
            Remarks = attendance.Remarks
        };
    }
}
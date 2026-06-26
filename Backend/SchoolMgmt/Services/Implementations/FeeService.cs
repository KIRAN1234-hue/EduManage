using Microsoft.EntityFrameworkCore;
using SchoolMgmt.Data;
using SchoolMgmt.DTOs.Fee;
using SchoolMgmt.Entities;
using SchoolMgmt.Enums;
using SchoolMgmt.Repositories.Interfaces;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Services.Implementations;

public class FeeService : IFeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;

    public FeeService(IUnitOfWork unitOfWork, AppDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<FeeStructureResponseDto> CreateFeeStructureAsync(
        CreateFeeStructureDto dto)
    {
        var exists = await _unitOfWork.FeeStructures.AnyAsync(f =>
            f.ClassId == dto.ClassId &&
            f.AcademicYear == dto.AcademicYear &&
            f.TermName == dto.TermName);

        if (exists)
            throw new InvalidOperationException(
                $"{dto.TermName} fee structure already exists for this class.");

        var fee = new FeeStructure
        {
            Id = Guid.NewGuid(),
            ClassId = dto.ClassId,
            AcademicYear = dto.AcademicYear,
            TermName = dto.TermName,
            Amount = dto.Amount,
            DueDate = dto.DueDate,
            DiscountAllowed = dto.DiscountAllowed,
            Description = dto.Description
        };

        await _unitOfWork.FeeStructures.AddAsync(fee);
        await _unitOfWork.SaveChangesAsync();

        return await BuildFeeStructureResponse(fee.Id);
    }

    public async Task<IEnumerable<FeeStructureResponseDto>> GetFeeStructuresAsync(
        string academicYear)
    {
        var structures = await _context.FeeStructures
            .Include(f => f.Class)
            .Include(f => f.FeePayments)          // FeePayments — matches entity
            .Where(f => f.AcademicYear == academicYear)
            .OrderBy(f => f.Class.Name)
            .ThenBy(f => f.TermName)
            .ToListAsync();

        return structures.Select(MapFeeStructure);
    }

    public async Task<IEnumerable<FeeStructureResponseDto>> GetClassFeeStructuresAsync(
        Guid classId, string academicYear)
    {
        var structures = await _context.FeeStructures
            .Include(f => f.Class)
            .Include(f => f.FeePayments)
            .Where(f => f.ClassId == classId && f.AcademicYear == academicYear)
            .OrderBy(f => f.TermName)
            .ToListAsync();

        return structures.Select(MapFeeStructure);
    }

    public async Task<FeePaymentResponseDto> RecordPaymentAsync(
        RecordPaymentDto dto, Guid recordedByUserId)
    {
        var feeStructure = await _unitOfWork.FeeStructures.GetByIdAsync(dto.FeeStructureId)
            ?? throw new KeyNotFoundException("Fee structure not found.");

        var student = await _unitOfWork.Students.GetByIdAsync(dto.StudentId)
            ?? throw new KeyNotFoundException("Student not found.");

        // Calculate remaining balance
        var alreadyPaid = await _context.FeePayments
            .Where(p => p.StudentId == dto.StudentId &&
                        p.FeeStructureId == dto.FeeStructureId)
            .SumAsync(p => p.Amount - p.DiscountAmount);

        var netNew = dto.Amount - dto.DiscountAmount;

        if (alreadyPaid + netNew > feeStructure.Amount)
            throw new InvalidOperationException(
                $"Payment exceeds remaining balance of ₹{feeStructure.Amount - alreadyPaid}.");

        // Generate receipt number
        var receiptNumber = $"RCP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        var payment = new FeePayment
        {
            Id = Guid.NewGuid(),
            StudentId = dto.StudentId,
            FeeStructureId = dto.FeeStructureId,
            Amount = dto.Amount,
            DiscountAmount = dto.DiscountAmount,
            PaymentDate = DateTime.UtcNow,       // PaymentDate — matches entity
            ReceiptNumber = receiptNumber,          // ReceiptNumber — matches entity
            PaymentMethod = dto.PaymentMethod,
            Remarks = dto.Remarks,
            ReceiptUrl = dto.ReceiptUrl,
            RecordedByUserId = recordedByUserId,
            Status = (alreadyPaid + netNew >= feeStructure.Amount)
                ? PaymentStatus.Paid
                : PaymentStatus.Partial
        };

        await _unitOfWork.FeePayments.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        return await BuildPaymentResponse(payment.Id);
    }

    public async Task<IEnumerable<FeePaymentResponseDto>> GetPaymentsByStructureAsync(
        Guid feeStructureId)
    {
        var payments = await _context.FeePayments
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Include(p => p.FeeStructure)
            .Include(p => p.RecordedBy)
            .Where(p => p.FeeStructureId == feeStructureId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        return payments.Select(MapPayment);
    }

    public async Task<StudentFeeStatusDto> GetStudentFeeStatusAsync(
        Guid studentId, string academicYear)
    {
        var student = await _context.Students
            .Include(s => s.User)
            .Include(s => s.Class)
            .FirstOrDefaultAsync(s => s.Id == studentId)
            ?? throw new KeyNotFoundException("Student not found.");

        var structures = await _context.FeeStructures
            .Include(f => f.FeePayments.Where(p => p.StudentId == studentId))
            .Where(f => f.ClassId == student.ClassId &&
                        f.AcademicYear == academicYear)
            .ToListAsync();

        var terms = structures.Select(f =>
        {
            var paid = f.FeePayments.Sum(p => p.Amount - p.DiscountAmount);
            var receipt = f.FeePayments
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefault()?.ReceiptNumber;

            return new FeeTermStatusDto
            {
                FeeStructureId = f.Id,
                TermName = f.TermName,
                Amount = f.Amount,
                Paid = paid,
                DueDate = f.DueDate,
                ReceiptNumber = receipt,
                Status = paid >= f.Amount ? "Paid"
                    : f.DueDate < DateOnly.FromDateTime(DateTime.Today) ? "Overdue"
                    : paid > 0 ? "Partial" : "Pending"
            };
        }).ToList();

        return new StudentFeeStatusDto
        {
            StudentName = student.User?.FullName ?? string.Empty,
            RollNumber = student.RollNumber,
            ClassName = student.Class != null
                ? $"{student.Class.Name} {student.Class.Section}" : string.Empty,
            Terms = terms,
            TotalDue = terms.Sum(t => t.Amount),
            TotalPaid = terms.Sum(t => t.Paid),
            Balance = terms.Sum(t => t.Amount - t.Paid)
        };
    }

    public async Task<StudentFeeStatusDto> GetMyFeeStatusAsync(
        Guid userId, string academicYear)
    {
        var student = await _unitOfWork.Students
            .FirstOrDefaultAsync(s => s.UserId == userId)
            ?? throw new KeyNotFoundException("Student profile not found.");

        return await GetStudentFeeStatusAsync(student.Id, academicYear);
    }

    private async Task<FeeStructureResponseDto> BuildFeeStructureResponse(Guid id)
    {
        var f = await _context.FeeStructures
            .Include(f => f.Class)
            .Include(f => f.FeePayments)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new KeyNotFoundException("Fee structure not found.");

        return MapFeeStructure(f);
    }

    private static FeeStructureResponseDto MapFeeStructure(FeeStructure f) => new()
    {
        Id = f.Id,
        ClassName = f.Class != null ? $"{f.Class.Name} {f.Class.Section}" : string.Empty,
        AcademicYear = f.AcademicYear,
        TermName = f.TermName,
        Amount = f.Amount,
        DueDate = f.DueDate,
        DiscountAllowed = f.DiscountAllowed,
        Description = f.Description,
        PaidCount = f.FeePayments?.Select(p => p.StudentId).Distinct().Count() ?? 0,
        TotalCollected = f.FeePayments?.Sum(p => p.Amount - p.DiscountAmount) ?? 0
    };

    private async Task<FeePaymentResponseDto> BuildPaymentResponse(Guid paymentId)
    {
        var p = await _context.FeePayments
            .Include(p => p.Student).ThenInclude(s => s.User)
            .Include(p => p.FeeStructure)
            .Include(p => p.RecordedBy)
            .FirstOrDefaultAsync(p => p.Id == paymentId)
            ?? throw new KeyNotFoundException("Payment not found.");

        return MapPayment(p);
    }

    private static FeePaymentResponseDto MapPayment(FeePayment p) => new()
    {
        Id = p.Id,
        StudentName = p.Student?.User?.FullName ?? string.Empty,
        RollNumber = p.Student?.RollNumber ?? string.Empty,
        TermName = p.FeeStructure?.TermName ?? string.Empty,
        Amount = p.Amount,
        DiscountAmount = p.DiscountAmount,
        PaymentDate = p.PaymentDate,
        ReceiptNumber = p.ReceiptNumber,
        ReceiptUrl = p.ReceiptUrl,
        Status = p.Status.ToString(),
        PaymentMethod = p.PaymentMethod,
        Remarks = p.Remarks,
        RecordedBy = p.RecordedBy?.FullName ?? string.Empty,
        TotalFee = p.FeeStructure?.Amount ?? 0,
        Balance = (p.FeeStructure?.Amount ?? 0) - (p.Amount - p.DiscountAmount)
    };
    public async Task<StudentFeeStatusDto> GetMyChildFeeStatusAsync(
    Guid parentUserId, string academicYear)
    {
        var parent = await _context.Parents
            .FirstOrDefaultAsync(p => p.UserId == parentUserId)
            ?? throw new KeyNotFoundException("Parent profile not found.");

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.ParentId == parent.Id);

        return await GetStudentFeeStatusAsync(student.Id, academicYear);
    }

    public async Task<FeePaymentResponseDto> ParentPaymentAsync(
        ParentPaymentDto dto, Guid parentUserId)
    {
        // Find parent → get their linked student
        var parent = await _context.Parents
            .FirstOrDefaultAsync(p => p.UserId == parentUserId)
            ?? throw new KeyNotFoundException("Parent profile not found.");

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.ParentId == parent.Id);

        // Reuse existing RecordPayment logic
        var recordDto = new RecordPaymentDto
        {
            StudentId = student.Id,
            FeeStructureId = dto.FeeStructureId,
            Amount = dto.Amount,
            DiscountAmount = 0,
            PaymentMethod = dto.PaymentMethod,
            Remarks = dto.Remarks,
            ReceiptUrl = null
        };

        // Parent's userId becomes RecordedByUserId
        return await RecordPaymentAsync(recordDto, parentUserId);
    }
    public async Task<StudentBalanceDto?> GetStudentBalanceAsync(
    Guid studentId, Guid feeStructureId)
    {
        var feeStructure = await _unitOfWork.FeeStructures
            .GetByIdAsync(feeStructureId);

        if (feeStructure == null) return null;

        var paid = await _context.FeePayments
            .Where(p => p.StudentId == studentId &&
                        p.FeeStructureId == feeStructureId)
            .SumAsync(p => p.Amount - p.DiscountAmount);

        return new StudentBalanceDto
        {
            TotalAmount = feeStructure.Amount,
            Paid = paid,
            Remaining = feeStructure.Amount - paid,
            IsFullyPaid = paid >= feeStructure.Amount
        };
    }
    public async Task<FeePaymentResponseDto> RecordParentPaymentAsync(
    RecordPaymentDto dto, Guid parentUserId)
    {
        // Resolve parent → child in service, not controller
        var student = await _context.Students
    .Include(s => s.Parent)
    .FirstOrDefaultAsync(s => s.Parent.UserId == parentUserId);

        if (student == null)
            throw new InvalidOperationException(
                "No child linked to this parent account.");

        dto.StudentId = student.Id;

        return await RecordPaymentAsync(dto, parentUserId);
    }
}
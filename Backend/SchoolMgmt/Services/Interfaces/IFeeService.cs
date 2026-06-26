using SchoolMgmt.DTOs.Fee;

namespace SchoolMgmt.Services.Interfaces;

public interface IFeeService
{
    Task<FeeStructureResponseDto> CreateFeeStructureAsync(CreateFeeStructureDto dto);
    Task<IEnumerable<FeeStructureResponseDto>> GetFeeStructuresAsync(string academicYear);
    Task<IEnumerable<FeeStructureResponseDto>> GetClassFeeStructuresAsync(Guid classId, string academicYear);
    Task<FeePaymentResponseDto> RecordPaymentAsync(RecordPaymentDto dto, Guid recordedByUserId);
    Task<IEnumerable<FeePaymentResponseDto>> GetPaymentsByStructureAsync(Guid feeStructureId);
    Task<StudentFeeStatusDto> GetStudentFeeStatusAsync(Guid studentId, string academicYear);
    Task<StudentFeeStatusDto> GetMyFeeStatusAsync(Guid userId, string academicYear);
    Task<StudentFeeStatusDto> GetMyChildFeeStatusAsync(Guid parentUserId, string academicYear);
    Task<FeePaymentResponseDto> ParentPaymentAsync(ParentPaymentDto dto, Guid parentUserId);    Task<StudentBalanceDto?> GetStudentBalanceAsync(Guid studentId, Guid feeStructureId);
    Task<FeePaymentResponseDto> RecordParentPaymentAsync(RecordPaymentDto dto, Guid parentUserId);
}
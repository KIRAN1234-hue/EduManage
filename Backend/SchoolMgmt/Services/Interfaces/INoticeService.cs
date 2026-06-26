using SchoolMgmt.DTOs.Notice;

namespace SchoolMgmt.Services.Interfaces;

public interface INoticeService
{
    Task<NoticeResponseDto> CreateNoticeAsync(
        CreateNoticeDto dto, Guid postedByUserId);

    Task<IEnumerable<NoticeResponseDto>> GetNoticesAsync(
        string userRole, Guid userId);

    Task AcknowledgeNoticeAsync(Guid noticeId, Guid userId);

    Task ArchiveNoticeAsync(Guid noticeId);  // soft delete

    Task DeleteNoticeAsync(Guid noticeId);   // hard delete
}
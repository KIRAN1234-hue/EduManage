using Microsoft.EntityFrameworkCore;
using SchoolMgmt.Data;
using SchoolMgmt.DTOs.Notice;
using SchoolMgmt.Entities;
using SchoolMgmt.Enums;
using SchoolMgmt.Repositories.Interfaces;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Services.Implementations;

public class NoticeService : INoticeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;

    public NoticeService(IUnitOfWork unitOfWork, AppDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<NoticeResponseDto> CreateNoticeAsync(
        CreateNoticeDto dto, Guid postedByUserId)
    {
        var notice = new Notice
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            Content = dto.Content,
            TargetRole = dto.TargetRole,         // NoticeTargetRole enum
            AttachmentUrl = dto.AttachmentUrl,
            PostedByUserId = postedByUserId,          // correct FK name
            PublishedAt = DateTime.UtcNow,         // correct property name
            IsArchived = false
        };

        await _unitOfWork.Notices.AddAsync(notice);
        await _unitOfWork.SaveChangesAsync();

        return await BuildNoticeResponse(notice.Id, postedByUserId);
    }

    public async Task<IEnumerable<NoticeResponseDto>> GetNoticesAsync(
        string userRole, Guid userId)
    {
        // Parse user role to NoticeTargetRole for filtering
        Enum.TryParse<NoticeTargetRole>(userRole, ignoreCase: true, out var targetRole);

        var notices = await _context.Notices
            .Include(n => n.CreatedBy)
            .Include(n => n.Acknowledgements)
            .Where(n =>
                !n.IsArchived &&
                (n.TargetRole == NoticeTargetRole.All ||
                 n.TargetRole == targetRole))
            .OrderByDescending(n => n.PublishedAt)
            .ToListAsync();

        return notices.Select(n => new NoticeResponseDto
        {
            Id = n.Id,
            Title = n.Title,
            Content = n.Content,
            TargetRole = n.TargetRole.ToString(),
            AttachmentUrl = n.AttachmentUrl,
            IsArchived = n.IsArchived,
            PostedBy = n.CreatedBy?.FullName ?? string.Empty,
            PublishedAt = n.PublishedAt,
            AcknowledgementCount = n.Acknowledgements.Count,
            IsAcknowledgedByMe = n.Acknowledgements.Any(a => a.UserId == userId)
        });
    }

    public async Task AcknowledgeNoticeAsync(Guid noticeId, Guid userId)
    {
        var alreadyAcknowledged = await _unitOfWork.NoticeAcknowledgements
            .AnyAsync(a => a.NoticeId == noticeId && a.UserId == userId);

        if (alreadyAcknowledged) return;

        var ack = new NoticeAcknowledgement
        {
            Id = Guid.NewGuid(),
            NoticeId = noticeId,
            UserId = userId,
            AcknowledgedAt = DateTime.UtcNow
        };

        await _unitOfWork.NoticeAcknowledgements.AddAsync(ack);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ArchiveNoticeAsync(Guid noticeId)
    {
        var notice = await _unitOfWork.Notices.GetByIdAsync(noticeId)
            ?? throw new KeyNotFoundException("Notice not found.");

        notice.IsArchived = true;
        _unitOfWork.Notices.Update(notice);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteNoticeAsync(Guid noticeId)
    {
        var notice = await _unitOfWork.Notices.GetByIdAsync(noticeId)
            ?? throw new KeyNotFoundException("Notice not found.");

        _unitOfWork.Notices.Remove(notice);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<NoticeResponseDto> BuildNoticeResponse(
        Guid noticeId, Guid userId)
    {
        var n = await _context.Notices
            .Include(n => n.CreatedBy)
            .Include(n => n.Acknowledgements)
            .FirstOrDefaultAsync(n => n.Id == noticeId)
            ?? throw new KeyNotFoundException("Notice not found.");

        return new NoticeResponseDto
        {
            Id = n.Id,
            Title = n.Title,
            Content = n.Content,
            TargetRole = n.TargetRole.ToString(),
            AttachmentUrl = n.AttachmentUrl,
            IsArchived = n.IsArchived,
            PostedBy = n.CreatedBy?.FullName ?? string.Empty,
            PublishedAt = n.PublishedAt,
            AcknowledgementCount = n.Acknowledgements.Count,
            IsAcknowledgedByMe = false
        };
    }
}
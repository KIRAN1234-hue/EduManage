using SchoolMgmt.Enums;

namespace SchoolMgmt.DTOs.Notice;

public class CreateNoticeDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NoticeTargetRole TargetRole { get; set; } = NoticeTargetRole.All;
    public string? AttachmentUrl { get; set; }
}
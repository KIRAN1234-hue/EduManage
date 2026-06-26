using SchoolMgmt.Enums;

namespace SchoolMgmt.DTOs.Notice;

public class NoticeResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string TargetRole { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public bool IsArchived { get; set; }
    public string PostedBy { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public bool IsAcknowledgedByMe { get; set; }
    public int AcknowledgementCount { get; set; }
}
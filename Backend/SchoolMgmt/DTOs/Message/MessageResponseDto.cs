namespace SchoolMgmt.DTOs.Message;

public class MessageResponseDto
{
    public Guid Id { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    //public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
    public Guid? ParentMessageId { get; set; }
    public int ReplyCount { get; set; }
    public IEnumerable<MessageResponseDto> Replies { get; set; }
        = new List<MessageResponseDto>();
}

public class ConversationDto
{
    public Guid OtherUserId { get; set; }
    public string OtherUserName { get; set; } = string.Empty;
    public string OtherUserRole { get; set; } = string.Empty;
    public string LastMessage { get; set; } = string.Empty;
    public DateTime LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}
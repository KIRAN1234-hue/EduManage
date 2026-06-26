namespace SchoolMgmt.DTOs.Message;

public class SendMessageDto
{
    public Guid ReceiverId { get; set; }
    //public string Subject { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = "General";
    public Guid? ParentMessageId { get; set; }
}
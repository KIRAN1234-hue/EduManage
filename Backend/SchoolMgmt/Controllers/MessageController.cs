using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolMgmt.DTOs.Message;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Controllers;

[ApiController]
[Route("api/messages")]
[Authorize]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessageController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    private Guid GetUserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id : Guid.Empty;

    // POST /api/messages
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        try
        {
            var result = await _messageService.SendMessageAsync(dto, userId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // GET /api/messages/conversations
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var conversations = await _messageService.GetMyConversationsAsync(userId);
        return Ok(conversations);
    }

    // GET /api/messages/conversation/{otherUserId}
    [HttpGet("conversation/{otherUserId}")]
    public async Task<IActionResult> GetConversation(Guid otherUserId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var messages = await _messageService
            .GetConversationAsync(userId, otherUserId);
        return Ok(messages);
    }

    // GET /api/messages/inbox
    [HttpGet("inbox")]
    public async Task<IActionResult> GetInbox()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var messages = await _messageService.GetInboxAsync(userId);
        return Ok(messages);
    }

    // GET /api/messages/sent
    [HttpGet("sent")]
    public async Task<IActionResult> GetSent()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var messages = await _messageService.GetSentAsync(userId);
        return Ok(messages);
    }

    // GET /api/messages/thread/{parentMessageId}
    [HttpGet("thread/{parentMessageId}")]
    public async Task<IActionResult> GetThread(Guid parentMessageId)
    {
        var messages = await _messageService.GetThreadAsync(parentMessageId);
        return Ok(messages);
    }

    // PUT /api/messages/{id}/read
    [HttpPut("{messageId}/read")]
    public async Task<IActionResult> MarkAsRead(Guid messageId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        await _messageService.MarkAsReadAsync(messageId, userId);
        return Ok(new { message = "Marked as read." });
    }

    // GET /api/messages/unread-count
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var count = await _messageService.GetUnreadCountAsync(userId);
        return Ok(new { count });
    }

    // DELETE /api/messages/{id}
    [HttpDelete("{messageId}")]
    public async Task<IActionResult> DeleteMessage(Guid messageId)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        try
        {
            await _messageService.DeleteMessageAsync(messageId, userId);
            return Ok(new { message = "Message deleted." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
    

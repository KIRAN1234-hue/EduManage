using Microsoft.EntityFrameworkCore;
using SchoolMgmt.Data;
using SchoolMgmt.DTOs.Notification;
using SchoolMgmt.Entities;
using SchoolMgmt.Repositories.Interfaces;
using SchoolMgmt.Services.Interfaces;

namespace SchoolMgmt.Services.Implementations;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppDbContext _context;

    public NotificationService(IUnitOfWork unitOfWork, AppDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task PushAsync(
        Guid userId,
        string title,
        string body,
        string type,
        string? actionUrl = null,
        string? relatedEntityId = null)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Body = body,
            NotificationType = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            ActionUrl = actionUrl,
            RelatedEntityId = relatedEntityId
        };

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task PushToManyAsync(
        IEnumerable<Guid> userIds,
        string title,
        string body,
        string type,
        string? actionUrl = null,
        string? relatedEntityId = null)
    {
        var notifications = userIds.Select(uid => new Notification
        {
            Id = Guid.NewGuid(),
            UserId = uid,
            Title = title,
            Body = body,
            NotificationType = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            ActionUrl = actionUrl,
            RelatedEntityId = relatedEntityId
        });

        foreach (var n in notifications)
            await _unitOfWork.Notifications.AddAsync(n);

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(Guid userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();

        return notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Body = n.Body,
            NotificationType = n.NotificationType,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt,
            ActionUrl = n.ActionUrl,
            RelatedEntityId = n.RelatedEntityId
        });
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _unitOfWork.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _unitOfWork.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null) return;

        notification.IsRead = true;
        _unitOfWork.Notifications.Update(notification);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var n in unread)
        {
            n.IsRead = true;
            _unitOfWork.Notifications.Update(n);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteNotificationAsync(Guid notificationId, Guid userId)
    {
        var notification = await _unitOfWork.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null) return;

        _unitOfWork.Notifications.Remove(notification);
        await _unitOfWork.SaveChangesAsync();
    }
}
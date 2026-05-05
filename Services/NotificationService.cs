using ClassroomApp.Data;
using ClassroomApp.Hubs;
using ClassroomApp.Models.Entities;
using ClassroomApp.Models.Enums;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomApp.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task CreateNotificationAsync(Guid userId, string title, string content,
            NotificationType type, Guid? relatedEntityId = null, string? relatedEntityType = null)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Content = content,
                Type = type,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = relatedEntityType,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(userId.ToString()).SendAsync("ReceiveNotification", new
            {
                notification.Id,
                notification.Title,
                notification.Content,
                Type = notification.Type.ToString(),
                notification.CreatedAt,
                notification.RelatedEntityId,
                notification.RelatedEntityType
            });
        }

        public async Task NotifyClassroomAsync(Guid classroomId, string title, string content,
            NotificationType type, Guid? relatedEntityId = null, string? relatedEntityType = null)
        {
            var studentUserIds = await _context.Students
                .Where(s => s.ClassroomId == classroomId)
                .Select(s => s.UserId)
                .ToListAsync();

            foreach (var userId in studentUserIds)
            {
                await CreateNotificationAsync(userId, title, content, type, relatedEntityId, relatedEntityType);
            }
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetUserNotificationCountAsync(Guid userId)
        {
            return await _context.Notifications.CountAsync(n => n.UserId == userId);
        }

        public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
                n.IsRead = true;

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        }
    }
}

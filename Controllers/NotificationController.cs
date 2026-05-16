using ClassroomApp.Filters;
using ClassroomApp.Models.ViewModels.Notification;
using ClassroomApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClassroomApp.Controllers
{
    [RoleAuthorize("Admin", "Teacher", "Student")]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var pageSize = 10;
            var notifications = await _notificationService.GetUserNotificationsAsync(userId, page, pageSize);
            var total = await _notificationService.GetUserNotificationCountAsync(userId);
            var unread = await _notificationService.GetUnreadCountAsync(userId);

            var vm = new NotificationViewModel
            {
                Notifications = notifications.Select(n => new NotificationItem
                {
                    Id = n.Id,
                    Title = n.Title,
                    Content = n.Content,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    RelatedEntityId = n.RelatedEntityId,
                    RelatedEntityType = n.RelatedEntityType
                }).ToList(),
                UnreadCount = unread,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _notificationService.MarkAsReadAsync(id, userId);
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            await _notificationService.MarkAllAsReadAsync(userId);
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Json(new { count });
        }

        [HttpGet]
        public async Task<IActionResult> GetRecent()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var notifications = await _notificationService.GetUserNotificationsAsync(userId, 1, 5);
            return Json(notifications.Select(n => new
            {
                n.Id,
                n.Title,
                n.Content,
                Type = n.Type.ToString(),
                n.IsRead,
                n.CreatedAt,
                n.RelatedEntityId,
                n.RelatedEntityType
            }));
        }

        [HttpGet]
        public IActionResult GetRedirectUrl(Guid notificationId, Guid relatedEntityId, string relatedEntityType)
        {
            var redirectUrl = "#";
            
            if (string.IsNullOrEmpty(relatedEntityType)) 
                return Json(new { url = redirectUrl });

            redirectUrl = relatedEntityType switch
            {
                "Assignment" => Url.Action("Details", "Assignment", new { id = relatedEntityId }),
                "Course" => Url.Action("Details", "Course", new { id = relatedEntityId }),
                "Message" => Url.Action("Index", "Message"),
                "Comment" => Url.Action("Details", "Assignment", new { id = relatedEntityId }),
                "Submission" => Url.Action("Details", "Assignment", new { id = relatedEntityId }),
                _ => "#"
            };

            return Json(new { url = redirectUrl });
        }
    }
}

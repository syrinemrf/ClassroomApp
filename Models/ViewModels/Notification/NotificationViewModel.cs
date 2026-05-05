using ClassroomApp.Models.Enums;

namespace ClassroomApp.Models.ViewModels.Notification
{
    public class NotificationViewModel
    {
        public List<NotificationItem> Notifications { get; set; } = new();
        public int UnreadCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
    }

    public class NotificationItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }

        public string TypeIcon => Type switch
        {
            NotificationType.DeadlineReminder => "fa-clock text-warning",
            NotificationType.NewAssignment => "fa-file-lines text-primary",
            NotificationType.NewCourse => "fa-book text-info",
            NotificationType.NewGrade => "fa-star text-success",
            NotificationType.NewMessage => "fa-envelope text-purple",
            NotificationType.NewComment => "fa-comment text-secondary",
            NotificationType.General => "fa-bell text-muted",
            _ => "fa-bell text-muted"
        };
    }
}

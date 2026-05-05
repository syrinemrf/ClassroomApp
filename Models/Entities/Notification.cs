using ClassroomApp.Models.Enums;

namespace ClassroomApp.Models.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }

        // Navigation
        public User User { get; set; } = null!;
    }
}

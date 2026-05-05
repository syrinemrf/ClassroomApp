using ClassroomApp.Models.Enums;

namespace ClassroomApp.Models.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? ProfilePicturePath { get; set; }
        public Role Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation
        public Teacher? Teacher { get; set; }
        public Student? Student { get; set; }
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<CalendarEvent> CreatedCalendarEvents { get; set; } = new List<CalendarEvent>();
    }
}

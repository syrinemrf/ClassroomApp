namespace ClassroomApp.Models.Entities
{
    public class CalendarEvent
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Color { get; set; } = "#4F46E5";
        public Guid CreatedByUserId { get; set; }
        public Guid? ClassroomId { get; set; }

        // Navigation
        public User CreatedBy { get; set; } = null!;
        public Classroom? Classroom { get; set; }
    }
}

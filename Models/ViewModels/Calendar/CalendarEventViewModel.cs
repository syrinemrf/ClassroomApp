namespace ClassroomApp.Models.ViewModels.Calendar
{
    public class CalendarEventViewModel
    {
        public Guid? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string? Color { get; set; }
        public string? EventType { get; set; }
        public Guid? ClassroomId { get; set; }
    }
}

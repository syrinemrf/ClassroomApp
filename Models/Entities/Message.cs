namespace ClassroomApp.Models.Entities
{
    public class Message
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        // Navigation
        public User Sender { get; set; } = null!;
        public User Receiver { get; set; } = null!;
    }
}

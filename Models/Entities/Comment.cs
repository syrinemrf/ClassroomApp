namespace ClassroomApp.Models.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid AssignmentId { get; set; }
        public Guid UserId { get; set; }
        public Guid? ParentCommentId { get; set; }

        // Navigation
        public Assignment Assignment { get; set; } = null!;
        public User User { get; set; } = null!;
        public Comment? ParentComment { get; set; }
        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}

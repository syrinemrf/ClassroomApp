namespace ClassroomApp.Models.Entities
{
    public class Assignment
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public int MaxScore { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Guid TeacherId { get; set; }
        public Guid ClassroomId { get; set; }
        public Guid? SubjectId { get; set; }

        // Navigation
        public Teacher Teacher { get; set; } = null!;
        public Classroom Classroom { get; set; } = null!;
        public Subject? Subject { get; set; }
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}

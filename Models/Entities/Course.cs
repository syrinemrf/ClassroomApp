namespace ClassroomApp.Models.Entities
{
    public class Course
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public Guid TeacherId { get; set; }
        public Guid SubjectId { get; set; }

        // Navigation
        public Teacher Teacher { get; set; } = null!;
        public Subject Subject { get; set; } = null!;
        public ICollection<CourseClassroom> CourseClassrooms { get; set; } = new List<CourseClassroom>();
    }
}

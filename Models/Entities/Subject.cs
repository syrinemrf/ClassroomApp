namespace ClassroomApp.Models.Entities
{
    /// <summary>
    /// Représente une matière enseignée (ex: Mathématiques, Informatique).
    /// Une matière est affectée à un enseignant et à plusieurs classes.
    /// </summary>
    public class Subject
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Color { get; set; } = "#4F46E5"; // couleur pour le calendrier
        public Guid TeacherId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Teacher Teacher { get; set; } = null!;
        public ICollection<SubjectClassroom> SubjectClassrooms { get; set; } = new List<SubjectClassroom>();
        public ICollection<Course> Courses { get; set; } = new List<Course>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}

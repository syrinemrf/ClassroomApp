namespace ClassroomApp.Models.Entities
{
    public class Teacher
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? Department { get; set; }
        public string? Bio { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
        public ICollection<Course> Courses { get; set; } = new List<Course>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public ICollection<Submission> GradedSubmissions { get; set; } = new List<Submission>();
    }
}

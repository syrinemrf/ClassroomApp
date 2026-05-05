namespace ClassroomApp.Models.Entities
{
    public class Student
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public Guid ClassroomId { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public Classroom Classroom { get; set; } = null!;
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }
}

namespace ClassroomApp.Models.Entities
{
    public class CourseClassroom
    {
        public Guid CourseId { get; set; }
        public Guid ClassroomId { get; set; }

        // Navigation
        public Course Course { get; set; } = null!;
        public Classroom Classroom { get; set; } = null!;
    }
}

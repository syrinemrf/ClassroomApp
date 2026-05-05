namespace ClassroomApp.Models.Entities
{
    public class Classroom
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string AcademicYear { get; set; } = string.Empty;

        // Navigation
        public ICollection<Student> Students { get; set; } = new List<Student>();
        public ICollection<SubjectClassroom> SubjectClassrooms { get; set; } = new List<SubjectClassroom>();
        public ICollection<CourseClassroom> CourseClassrooms { get; set; } = new List<CourseClassroom>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public ICollection<CalendarEvent> CalendarEvents { get; set; } = new List<CalendarEvent>();
    }
}

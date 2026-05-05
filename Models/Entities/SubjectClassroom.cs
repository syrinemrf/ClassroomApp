namespace ClassroomApp.Models.Entities
{
    /// <summary>
    /// Table de jointure many-to-many entre Subject et Classroom.
    /// </summary>
    public class SubjectClassroom
    {
        public Guid SubjectId { get; set; }
        public Guid ClassroomId { get; set; }

        // Navigation
        public Subject Subject { get; set; } = null!;
        public Classroom Classroom { get; set; } = null!;
    }
}

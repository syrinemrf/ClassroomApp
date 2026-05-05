using ClassroomApp.Models.Enums;

namespace ClassroomApp.Models.Entities
{
    public class Submission
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid AssignmentId { get; set; }
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public long? FileSize { get; set; }
        public DateTime SubmittedAt { get; set; }
        public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;
        public int? Score { get; set; }
        public string? TeacherComment { get; set; }
        public DateTime? GradedAt { get; set; }
        public Guid? GradedByTeacherId { get; set; }

        // Navigation
        public Student Student { get; set; } = null!;
        public Assignment Assignment { get; set; } = null!;
        public Teacher? GradedBy { get; set; }
    }
}

using ClassroomApp.Models.Enums;

namespace ClassroomApp.Models.ViewModels.Assignment
{
    public class AssignmentViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public int MaxScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string ClassroomName { get; set; } = string.Empty;
        public Guid ClassroomId { get; set; }
        public Guid? SubjectId { get; set; }
        public string? SubjectName { get; set; }
        public Guid TeacherId { get; set; }
        public int TotalStudents { get; set; }
        public int SubmissionCount { get; set; }
        public int GradedCount { get; set; }

        // Optional teacher attachment
        public string? AttachmentFileName { get; set; }
        public string? AttachmentFilePath { get; set; }
        public long? AttachmentFileSize { get; set; }

        // Student-specific
        public SubmissionStatus? MySubmissionStatus { get; set; }
        public int? MyScore { get; set; }
        public string? MyTeacherComment { get; set; }
        public Guid? MySubmissionId { get; set; }

        public string DeadlineBadgeClass =>
            DateTime.UtcNow > Deadline ? "bg-danger" :
            (Deadline - DateTime.UtcNow).TotalHours < 24 ? "bg-warning text-dark" :
            "bg-success";

        public string DeadlineStatus =>
            DateTime.UtcNow > Deadline ? "Expired" :
            (Deadline - DateTime.UtcNow).TotalHours < 24 ? "< 24h left" :
            "Open";

        public List<SubmissionDetailItem> Submissions { get; set; } = new();
        public List<CommentItem> Comments { get; set; } = new();
    }

    public class SubmissionDetailItem
    {
        public Guid StudentId { get; set; }
        public Guid SubmissionId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
        public SubmissionStatus Status { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public int? Score { get; set; }
        public int MaxScore { get; set; }
        public string? FileName { get; set; }
    }

    public class CommentItem
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? UserProfilePicture { get; set; }
        public string UserRole { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid UserId { get; set; }
        public Guid? ParentCommentId { get; set; }
        public List<CommentItem> Replies { get; set; } = new();
    }
}

using ClassroomApp.Models.Enums;

namespace ClassroomApp.Models.ViewModels.Submission
{
    public class MySubmissionsViewModel
    {
        public List<SubmissionItem> Submissions { get; set; } = new();
        public SubmissionStatus? FilterStatus { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
    }

    public class SubmissionItem
    {
        public Guid Id { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public string ClassroomName { get; set; } = string.Empty;
        public DateTime? SubmittedAt { get; set; }
        public SubmissionStatus Status { get; set; }
        public int? Score { get; set; }
        public int MaxScore { get; set; }
        public string? TeacherComment { get; set; }
        public string? FileName { get; set; }
        public DateTime Deadline { get; set; }
    }
}

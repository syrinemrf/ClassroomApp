using System.ComponentModel.DataAnnotations;

namespace ClassroomApp.Models.ViewModels.Submission
{
    public class GradeSubmissionViewModel
    {
        public Guid SubmissionId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
        public string AssignmentTitle { get; set; } = string.Empty;
        public int MaxScore { get; set; }
        public string? FileName { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string Status { get; set; } = string.Empty;

        [Required(ErrorMessage = "Score is required")]
        [Range(0, 1000, ErrorMessage = "Score must be between 0 and the max score")]
        public int Score { get; set; }

        [StringLength(2000)]
        public string? TeacherComment { get; set; }
    }
}
